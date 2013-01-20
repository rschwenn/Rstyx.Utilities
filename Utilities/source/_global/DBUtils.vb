
Imports System.Data
Imports System.Data.OleDb
Imports System.IO

'Namespace System
    
    ''' <summary> Static utility methods for dealing with Databases. </summary>
    Public NotInheritable Class DBUtils
        
        #Region "Private Fields"
            
            'Private Logger As Rstyx.LoggingConsole.Logger = Rstyx.LoggingConsole.LogBox.getLogger(MyClass.GetType.FullName)
            Private Shared Logger As Rstyx.LoggingConsole.Logger = Rstyx.LoggingConsole.LogBox.getLogger("Rstyx.Utilities.DBUtils")
            
        #End Region
        
        #Region "Constructor"
            
            Private Sub New
                'Hides the Constructor
            End Sub
            
        #End Region
        
        #Region "Enums"
            
            '' <summary> Sorting types supported by sorting methods of this class. </summary>
            'Public Enum SortType As Integer
            '    
            '    ''' <summary> Numeric if possible, otherwise alphanumeric </summary>
            '    Numeric      = 0
            '    
            '    ''' <summary> Alphanumeric </summary>
            '    Alphanumeric = 1
            'End Enum
            
        #End Region
        
        #Region "Public Static Methods"
            
            ''' <summary> Opens a MS Jet connection to an Excel worksheet. </summary>
             ''' <param name="XlFilePath"> Full path of Excel worksheet. </param>
             ''' <returns>                 The open <see cref="OleDbConnection"/>, or Null. </returns>
             ''' <remarks>
             ''' <para>
             ''' The first row of every Excel table must contain field names.
             ''' </para>
             ''' <para>
             ''' The Excel worksheet's format is guessed as "97 - 2003".
             ''' </para>
             ''' <para>
             ''' If a column has values of mixed data type, all values are read as "text".
             ''' </para>
             ''' </remarks>
            Public Shared Function connectToExcelWorkbook(byVal XlFilePath As String) As OleDbConnection
                Dim DBconn  As OleDbConnection = Nothing
                Try
                    Logger.logDebug(StringUtils.sprintf("connectToExcelWorkbook(): Establish DB conection. XLS='%s'.", XlFilePath))
                    
                    if (Not File.Exists(XlFilePath)) then
                        Logger.logError(StringUtils.sprintf("connectToExcelWorkbook(): Excel-Arbeitsmappe '%s' nicht gefunden!", XlFilePath))
                    else
                        ' Jet/Excel setting: Check all lines to determine the column's data type.
                        RegUtils.setValue("HKEY_LOCAL_MACHINE\SOFTWARE\Microsoft\Jet\4.0\Engines\Excel\TypeGuessRows", 0, Microsoft.Win32.RegistryValueKind.DWord)
                        
                        ' Init DB connection. (Jet/Excel setting "IMEX=1": If a column has mixed data type, all values are read as "text").
                        Dim CSB As OleDbConnectionStringBuilder = New OleDbConnectionStringBuilder()
                        CSB.DataSource = XlFilePath
                        CSB.Provider   = "Microsoft.Jet.OLEDB.4.0"
                        CSB.Add("Extended Properties", "Excel 8.0;HDR=Yes;IMEX=1;")
                        DBconn = New OleDbConnection(CSB.ConnectionString)
                        DBconn.Open()
                        
                        ' Debug.
                        Logger.logDebug(StringUtils.sprintf("connectToExcelWorkbook(): DB conection status = '%s'.", DBconn.State.ToString()))
                        
                        If (DBconn.State = ConnectionState.Open) Then
                            Dim TableInfo As DataTable = DBconn.GetOleDbSchemaTable(OleDbSchemaGuid.Tables, New Object() {Nothing, Nothing, Nothing, "TABLE"})
                            Logger.logDebug(StringUtils.sprintf("%d Tables found:", TableInfo.Rows.Count))
                            For Each Row As DataRow In TableInfo.Rows
                                Logger.logDebug(StringUtils.sprintf("- %s", Row("TABLE_NAME")))
                            Next
                        End if
                    end if
                Catch ex as System.Exception
                    Logger.logError(ex, StringUtils.sprintf("connectToExcelWorkbook(): Fehler beim Aufbau der DB-Berbindung zu '%s'", XlFilePath))
                End Try
                
                Return DBconn
            End function
            
            ''' <summary> Gets a whole Table from the given Excel worksheet. </summary>
             ''' <param name="XlFilePath"> Full path of Excel worksheet. </param>
             ''' <param name="TableName">  The name of the table to get. May or may not enclosed in square brackets and/or end with "$". </param>
             ''' <returns>                 The DataTable. May be empty. </returns>
            Public Shared Function getExcelSheet(byVal TableName As String, byVal XlFilePath As String) As DataTable
                Dim Table  As New DataTable()
                Try
                    TableName = TableName.ReplaceWith("^\[", "").ReplaceWith("\$?\]?$", "")
                    TableName &= "$"
                    
                    Logger.logDebug(StringUtils.sprintf("getExcelSheet(): Try to get table '%s' from Excel workbook '%s'.", TableName, XlFilePath))
                    
                    Dim XLconn As OleDbConnection = DBUtils.connectToExcelWorkbook(XlFilePath)
                    
                    If (XLconn isNot Nothing) then
                        if (Not (XLconn.state = ConnectionState.Open)) then
                            Logger.logError("getExcelSheet(): Die Datenbank-Verbindung zu Excel existiert, ist aber nicht geöffnet!")
                        else
                            Dim DBcmd As OleDbCommand = New OleDbCommand(TableName, XLconn)
                            DBcmd.CommandType = CommandType.TableDirect
                            Dim DBreader As OleDbDataReader = DBcmd.ExecuteReader()
                            
                            Table.Load(DBreader)
                            Table.TableName = TableName
                            
                            DBreader.Close()
                            DBcmd.Dispose()
                            XLconn.Close()
                            XLconn.Dispose()
                        end if
                    End if
                Catch ex as System.Exception
                    Logger.logError(ex, StringUtils.sprintf("getExcelSheet(): Fehler beim Holen der Tabelle '%s' aus Excel Arbeitsmappe '%s'.", TableName, XlFilePath))
                End Try
                
                Return Table
            End function
            
        #End Region
        
        #Region "Private Members"
            
            
        #End Region
        
    End Class
    
    ''' <summary> Extension methods for System.Data types. </summary>
    Public Module DBExtensions
        
        #Region "Private Fields"
            
            Private Logger As Rstyx.LoggingConsole.Logger = Rstyx.LoggingConsole.LogBox.getLogger("Rstyx.Utilities.DBExtensions")
            
        #End Region
        
        #Region "DataTable Extensions"
            
            ''' <summary> Checks, if a certain field name exists (case sensitive) in a given table. </summary>
             ''' <param name="Table">      The table to check. </param>
             ''' <param name="FieldName">  The field name of interest. </param>
             ''' <returns>                 True, if the field has been found in the table. </returns>
             <System.Runtime.CompilerServices.Extension()> 
            Public Function containsField(Table As DataTable,
                                          byVal FieldName As String
                                          ) As Boolean
                Return containsField(Table, FieldName, ignoreCase:=False, logError:=False)
            End Function
            
            ''' <summary> Checks, if a certain field name exists in a given table. </summary>
             ''' <param name="Table">      The table to check. </param>
             ''' <param name="FieldName">  The field name of interest. </param>
             ''' <param name="ignoreCase"> If True, comparing field names is case insensitive. </param>
             ''' <param name="logError">   If True, an error message is logged, if the field doesn't exist. </param>
             ''' <returns>                 True, if the field has been found in the table. </returns>
             <System.Runtime.CompilerServices.Extension()> 
            Public Function containsField(Table As DataTable,
                                          byVal FieldName As String,
                                          byVal ignoreCase As Boolean,
                                          byVal logError As Boolean
                                          ) As Boolean
                Dim success  As Boolean = false
                Try
                    if (Not Table is Nothing) then
                        Dim DBColumnName As String
                        Logger.logDebug(StringUtils.sprintf("containsField(): Checks presence of field '%s' in table '%s'.", FieldName, Table.TableName))
                        
                        For Each Column As DataColumn In Table.Columns
                            
                            If (ignoreCase) Then
                                FieldName    = FieldName.ToLower()
                                DBColumnName = Column.ColumnName.ToLower()
                            Else
                                DBColumnName = Column.ColumnName
                            End If
                            
                            If (DBColumnName = FieldName) Then
                                success = True
                                Logger.logDebug(StringUtils.sprintf("containsField(): Found field '%s' in table '%s'.", FieldName, Table.TableName))
                                Exit For
                            End If
                        Next
                        If (logError and (not success)) Then Logger.logError(StringUtils.sprintf("containsField(): Das Feld '%s' existiert nicht in der Tabelle!", FieldName))
                    end if
                    
                Catch ex as System.Exception
                    Logger.logError(ex, "containsField(): unerwarteter Fehler.")
                End Try
                
                Return success
            End Function
            
        #End Region
        
        #Region "OleDbConnection Extensions"
            
            ''' <summary> Gets a whole table from the given connection. </summary>
             ''' <param name="DBconn">    An established <see cref="OleDbConnection"/> to use. It will be opened if it isn't yet. </param>
             ''' <param name="TableName"> The name of the table to get. An Excel table name must end with "$". </param>
             ''' <returns>                The DataTable. May be empty. </returns>
             <System.Runtime.CompilerServices.Extension()> 
            Public Function getTable(DBconn As OleDbConnection, byVal TableName As String) As DataTable
                Dim Table  As New DataTable()
                Try
                    Logger.logDebug(StringUtils.sprintf("getTable(): Try to get table '%s' from OleDbConnection.", TableName))
                    If (DBConn is nothing) then
                        Logger.logError("getTable(): Die Datenbank-Verbindung existiert nicht!")
                    Else
                        Dim hasBeenOpened  As Boolean = (DBConn.state = ConnectionState.Open)
                        
                        If (not hasBeenOpened) Then
                            Logger.logDebug("getTable(): DB connection exists, but isn't open => try to open.")
                            DBConn.Open()
                        End If
                        
                        if (not (DBConn.state = ConnectionState.Open)) then
                            Logger.logError("getTable(): Die Datenbank-Verbindung existiert, ist aber nicht geöffnet und konnte auch nicht geöffnet werden!")
                        else
                            Dim DBcmd As OleDbCommand = New OleDbCommand(TableName, DBconn)
                            DBcmd.CommandType = CommandType.TableDirect
                            Dim DBreader As OleDbDataReader = DBcmd.ExecuteReader()
                            
                            Table.Load(DBreader)
                            Table.TableName = TableName
                            
                            DBreader.Close()
                            If (not hasBeenOpened) Then DBConn.Close()
                        end if
                    End if
                Catch ex as System.Exception
                    Logger.logError(ex, StringUtils.sprintf("getTable(): Fehler beim Holen der Tabelle.", TableName))
                End Try
                
                Return Table
            End function
                
            ''' <summary> Executes an SQL query on the <see cref="OleDbConnection"/>. </summary>
             ''' <param name="DBconn"> An established <see cref="OleDbConnection"/> to use. It will be opened if it isn't yet. </param>
             ''' <param name="SQL">    The query string. </param>
             ''' <returns>             The query result as DataTable. May be empty. </returns>
             <System.Runtime.CompilerServices.Extension()> 
            Public Function query(DBconn As OleDbConnection, byVal SQL As String) As DataTable
                Dim Table  As New DataTable()
                Try
                    Logger.logDebug(StringUtils.sprintf("query(): Try to query SQL '%s'.", SQL))
                    If (DBConn is nothing) then
                        Logger.logError("query(): Die Datenbank-Verbindung existiert nicht!")
                    Else
                        Dim hasBeenOpened  As Boolean = (DBConn.state = ConnectionState.Open)
                        
                        If (not hasBeenOpened) Then
                            Logger.logDebug("query(): DB connection exists, but isn't open => try to open.")
                            DBConn.Open()
                        End If
                        
                        if (not (DBConn.state = ConnectionState.Open)) then
                            Logger.logError("query(): Die Datenbank-Verbindung existiert, ist aber nicht geöffnet und konnte auch nicht geöffnet werden!")
                        else
                            Dim DBcmd As OleDbCommand = New OleDbCommand(SQL, DBconn)
                            Dim DBreader As OleDbDataReader = DBcmd.ExecuteReader()
                            
                            Table.Load(DBreader)
                            'Table.TableName = TableName
                            
                            DBreader.Close()
                            If (not hasBeenOpened) Then DBConn.Close()
                        end if
                    End if
                Catch ex as System.Exception
                    Logger.logError(ex, "query(): Fehler bei der SQL-Abfrage")
                End Try
                
                Return Table
            End function
            
            ''' <summary> Checks, if a given field name exists in a given table. </summary>
             ''' <param name="DBconn">    An established <see cref="OleDbConnection"/> to use. It will be opened if it isn't yet. </param>
             ''' <param name="FieldName"> The field name of interest. </param>
             ''' <param name="TableName"> The table to check. (An Excel table name must end with "$" and must not enclosed in square brackets!) </param>
             ''' <param name="logError">  If True, an error message is logged, if the field doesn't exist. </param>
             ''' <returns>                True, if the field has been found in the table. </returns>
             <System.Runtime.CompilerServices.Extension()> 
            Public Function TableContainsField(DBconn As OleDbConnection,
                                              byVal TableName As String,
                                              byVal FieldName As String,
                                              Optional byVal logError As Boolean = false
                                              ) As Boolean
                dim success  As Boolean = false
                Try
                    Logger.logDebug(StringUtils.sprintf("TableContainsField(): Checks presence of field '%s' in table '%s'.", FieldName, TableName))
                    
                    if (DBConn is Nothing) then
                        Logger.logError("TableContainsField(): Die Datenbank-Verbindung existiert nicht!")
                    else
                        Dim hasBeenOpened  As Boolean = (DBConn.state = ConnectionState.Open)
                        
                        If (not hasBeenOpened) Then
                            Logger.logDebug("TableContainsField(): DB connection exists, but isn't open => try to open.")
                            DBConn.Open()
                        End If
                        
                        if (not (DBConn.state = ConnectionState.Open)) then 
                            Logger.logError("TableContainsField(): Die Datenbank-Verbindung existiert, ist aber nicht geöffnet und konnte auch nicht geöffnet werden!")
                        else
                            Dim FieldInfo As DataTable = DBconn.GetOleDbSchemaTable(OleDbSchemaGuid.Columns, New Object() {Nothing, Nothing, TableName, FieldName})
                            if (FieldInfo.Rows.Count = 1) then 
                                Logger.logDebug(StringUtils.sprintf("TableContainsField(): Found field '%s' in table '%s'.", FieldName, TableName))
                                success = true
                            End If
                            'For Each Row As DataRow In FieldInfo.Rows
                            '    Logger.logDebug(StringUtils.sprintf("- Feldname='%s' (Tabelle=%s)", Row("COLUMN_NAME"), Row("TABLE_NAME")))
                            'Next
                            If (not hasBeenOpened) Then DBConn.Close()
                        end if
                    end if
                    
                    if (logError and (not success)) then Logger.logError(StringUtils.sprintf("Das Feld '%s' existiert nicht in der Tabelle!", FieldName))
                        
                Catch ex as System.Exception
                    Logger.logError(ex, "TableContainsField(): unerwarteter Fehler.")
                End Try
                
                Return success
            End function
            
        #End Region
        
    End Module
    
'End Namespace

' for jEdit:  :collapseFolds=2::tabSize=4::indentSize=4:
