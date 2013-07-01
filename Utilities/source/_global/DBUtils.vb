
Imports System.Data
Imports System.Data.OleDb
Imports System.IO

'Namespace System
    
    ''' <summary> Static utility methods for dealing with Databases. </summary>
    Public NotInheritable Class DBUtils
        
        #Region "Private Fields"
            
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
             ''' <returns>                 The <see cref="OleDbConnection"/> which should be opened. </returns>
             ''' <exception cref="T:System.IO.FileNotFoundException"> <paramref name="XlFilePath"/> hasn't been found (may be empty or invalid). </exception>
             ''' <exception cref="T:Rstyx.Utilities.RemarkException"> Wraps any exception with a clear message. </exception>
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
                
                Logger.logDebug(StringUtils.sprintf("connectToExcelWorkbook(): Try to establish DB conection to Excel workbook '%s'.", XlFilePath))
                
                If (Not File.Exists(XlFilePath)) Then Throw New System.IO.FileNotFoundException(Rstyx.Utilities.Resources.Messages.DBUtils_ExcelWorkbookNotFound, XlFilePath)
                
                Dim DBconn  As OleDbConnection = Nothing
                Try
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
                    Logger.logDebug(StringUtils.sprintf("connectToExcelWorkbook(): Established DB conection status = '%s'.", DBconn.State.ToString()))
                    
                    If (DBconn.State = ConnectionState.Open) Then
                        Dim TableInfo As DataTable = DBconn.GetOleDbSchemaTable(OleDbSchemaGuid.Tables, New Object() {Nothing, Nothing, Nothing, "TABLE"})
                        Logger.logDebug(StringUtils.sprintf("%d Tables found:", TableInfo.Rows.Count))
                        For Each Row As DataRow In TableInfo.Rows
                            Logger.logDebug(StringUtils.sprintf("- %s", Row("TABLE_NAME")))
                        Next
                    End if
                    
                    Return DBconn
                    
                Catch ex As System.Exception
                    ' <exception cref="T:System.InvalidOperationException"> The OLEDB provider "Microsoft.Jet.OLEDB.4.0" isn't available. </exception>
                    ' Clean-up resources, then re-throw with remark message.
                    If (DBconn IsNot Nothing) Then
                        DBconn.Dispose()
                        DBconn = Nothing
                    End If
                    Throw New RemarkException(StringUtils.sprintf(Rstyx.Utilities.Resources.Messages.DBUtils_ConnectionToExcelWorkbookFailed, XlFilePath), ex)
                End Try
            End Function
            
            ''' <summary> Gets a whole Table from a given Excel worksheet. </summary>
             ''' <param name="XlFilePath"> Full path of Excel worksheet. </param>
             ''' <param name="TableName">  The name of the table to get. May or may not enclosed in square brackets and/or end with "$". </param>
             ''' <returns>                 The DataTable. </returns>
             ''' <exception cref="T:System.ArgumentNullException"> <paramref name="TableName"/> is <see langword="null"/> or empty. </exception>
             ''' <exception cref="T:System.IO.FileNotFoundException"> <paramref name="XlFilePath"/> hasn't been found (may be empty or invalid). </exception>
             ''' <exception cref="T:Rstyx.Utilities.RemarkException"> Wraps any exception with a clear message. </exception>
            Public Shared Function getExcelSheet(byVal TableName As String, byVal XlFilePath As String) As DataTable
                
                If (TableName.IsEmptyOrWhiteSpace()) Then Throw New System.ArgumentNullException("TableName")
                If (Not File.Exists(XlFilePath)) Then Throw New System.IO.FileNotFoundException(Rstyx.Utilities.Resources.Messages.DBUtils_ExcelWorkbookNotFound, XlFilePath)
                Try
                    TableName  = TableName.ReplaceWith("^\[", "").ReplaceWith("\$?\]?$", "")
                    TableName &= "$"
                    Logger.logDebug(StringUtils.sprintf("getExcelSheet(): Try to get table '%s' from Excel workbook '%s'.", TableName, XlFilePath))
                    
                    Using XLconn As OleDbConnection = connectToExcelWorkbook(XlFilePath)
                        Return XLconn.getTable(TableName)
                    End Using
                    
                Catch ex As System.Exception
                    ' <exception cref="T:System.InvalidOperationException"> The OLEDB provider "Microsoft.Jet.OLEDB.4.0" isn't available. </exception>
                    Throw New RemarkException(StringUtils.sprintf(Rstyx.Utilities.Resources.Messages.DBUtils_OpenExcelWorksheetFailed, TableName, XlFilePath), ex)
                End Try
            End Function
            
        #End Region
        
    End Class
    
    ''' <summary> Extension methods for System.Data types. </summary>
    Public Module DBExtensions
        
        #Region "Private Fields"
            
            Private Logger As Rstyx.LoggingConsole.Logger = Rstyx.LoggingConsole.LogBox.getLogger("Rstyx.Utilities.DBExtensions")
            
        #End Region
        
        #Region "DataTable Extensions"
            
            ''' <summary> Checks, if a certain field name exists in a given table. </summary>
             ''' <param name="Table">      The table to check. </param>
             ''' <param name="FieldName">  The field name of interest. </param>
             ''' <exception cref="T:System.ArgumentNullException"> <paramref name="Table"/> is <see langword="null"/>. </exception>
             ''' <exception cref="T:System.ArgumentNullException"> <paramref name="FieldName"/> is <see langword="null"/> or empty. </exception>
             ''' <returns>                 <see langword="true"/>, if the field has been found in the table, otherwise <see langword="false"/>. </returns>
             <System.Runtime.CompilerServices.Extension()> 
            Public Function containsField(Table As DataTable, byVal FieldName As String) As Boolean
                
                If (Table Is Nothing) Then Throw New System.ArgumentNullException("Table")
                If (FieldName.IsEmptyOrWhiteSpace()) Then Throw New System.ArgumentNullException("FieldName")
                
                Dim success  As Boolean = false
                Logger.logDebug(StringUtils.sprintf("containsField(): Checks presence of field '%s' in table '%s'.", FieldName, Table.TableName))
                
                success = Table.Columns.Contains(FieldName)
                
                Logger.logDebug(StringUtils.sprintf("containsField(): Found field '%s' in table '%s'.", FieldName, Table.TableName))
                
                Return success
            End Function
            
            ''' <summary> Verifies that a certain field name exists in a given table. </summary>
             ''' <param name="Table">      The table to check. </param>
             ''' <param name="FieldName">  The field name of interest. </param>
             ''' <exception cref="T:System.ArgumentNullException"> <paramref name="Table"/> is <see langword="null"/>. </exception>
             ''' <exception cref="T:System.ArgumentNullException"> <paramref name="FieldName"/> is <see langword="null"/> or empty. </exception>
             ''' <exception cref="T:RemarkException"> Thrown if the field name hasn't been found in the table. </exception>
             <System.Runtime.CompilerServices.Extension()> 
            Public Sub VerifyField(Table As DataTable, byVal FieldName As String)
                
                If (Table Is Nothing) Then Throw New System.ArgumentNullException("Table")
                If (FieldName.IsEmptyOrWhiteSpace()) Then Throw New System.ArgumentNullException("FieldName")
                
                If (Not Table.containsField(FieldName)) Then
                    Throw New RemarkException(StringUtils.sprintf(Rstyx.Utilities.Resources.Messages.DBUtils_FieldNotFoundInTable, FieldName, Table.TableName))
                End If
            End Sub
            
        #End Region
        
        #Region "OleDbConnection Extensions"
            
            ''' <summary> Gets a whole table from a given connection. </summary>
             ''' <param name="DBconn">    An established <see cref="OleDbConnection"/> to use. </param>
             ''' <param name="TableName"> The name of the table to get. (An Excel table name must end with "$".) </param>
             ''' <returns>                The DataTable. </returns>
             ''' <exception cref="T:System.ArgumentNullException"> <paramref name="DBconn"/> is <see langword="null"/>. </exception>
             ''' <exception cref="T:System.ArgumentNullException"> <paramref name="TableName"/> is <see langword="null"/> or empty. </exception>
             ''' <exception cref="T:System.Data.OleDb.OleDbException"> Database error opening table. </exception>
             <System.Runtime.CompilerServices.Extension()> 
            Public Function getTable(DBconn As OleDbConnection, byVal TableName As String) As DataTable
                
                If (DBConn Is Nothing) Then Throw New System.ArgumentNullException("DBConn")
                If (TableName.IsEmptyOrWhiteSpace()) Then Throw New System.ArgumentNullException("TableName")
                
                Logger.logDebug(StringUtils.sprintf("getTable(): Try to get table '%s' from OleDbConnection.", TableName))
                
                Using DBcmd As OleDbCommand = New OleDbCommand(TableName, DBconn)
                    DBcmd.CommandType = CommandType.TableDirect
                    Using DBreader As OleDbDataReader = DBcmd.ExecuteReader()
                        Dim Table As New DataTable()
                        Table.Load(DBreader)
                        Table.TableName = TableName
                        Logger.logDebug(StringUtils.sprintf("getTable(): Successfully got table '%s' from OleDbConnection.", TableName))
                        Return Table
                    End Using
                End Using
            End Function
                
            ''' <summary> Executes an SQL query on the <see cref="OleDbConnection"/>. </summary>
             ''' <param name="DBconn"> An established <see cref="OleDbConnection"/> to use. It will be opened if it isn't yet. </param>
             ''' <param name="SQL">    The query string. </param>
             ''' <returns>             The query result as DataTable. May be empty. </returns>
             ''' <exception cref="T:System.ArgumentNullException"> <paramref name="DBconn"/> is <see langword="null"/>. </exception>
             ''' <exception cref="T:System.ArgumentNullException"> <paramref name="SQL"/> is <see langword="null"/> or empty. </exception>
             ''' <exception cref="T:System.Data.OleDb.OleDbException"> Database error. </exception>
             <System.Runtime.CompilerServices.Extension()> 
            Public Function query(DBconn As OleDbConnection, byVal SQL As String) As DataTable
                
                If (DBConn Is Nothing) Then Throw New System.ArgumentNullException("DBConn")
                If (SQL.IsEmptyOrWhiteSpace()) Then Throw New System.ArgumentNullException("SQL")
                
                Logger.logDebug(StringUtils.sprintf("query(): Try to query SQL '%s'.", SQL))
                
                Using DBcmd As OleDbCommand = New OleDbCommand(SQL, DBconn)
                    DBcmd.CommandType = CommandType.Text
                    Using DBreader As OleDbDataReader = DBcmd.ExecuteReader()
                        Dim Table As New DataTable()
                        Table.Load(DBreader)
                        Logger.logDebug(StringUtils.sprintf("query(): Successfully executed SQL query '%s'.", SQL))
                        Return Table
                    End Using
                End Using
            End Function
            
        #End Region
        
    End Module
    
'End Namespace

' for jEdit:  :collapseFolds=2::tabSize=4::indentSize=4:
