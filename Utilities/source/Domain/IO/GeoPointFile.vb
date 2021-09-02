
Imports System
Imports System.Collections.Generic
Imports System.Collections.ObjectModel
Imports System.IO
Imports System.Text

Imports Rstyx.Utilities
Imports Rstyx.Utilities.Collections
Imports Rstyx.Utilities.Domain
Imports Rstyx.Utilities.IO
Imports Rstyx.Utilities.StringUtils

Namespace Domain.IO
    
    ''' <summary> The base class for reading and writing GeoPoint files. </summary>
     ''' <remarks>
     ''' A derived class will support read and write exactly one discrete file type.
     ''' <para>
     ''' <b>Features:</b>
     ''' <list type="bullet">
     ''' <item><description> The <see cref="GeoPointFile.PointStream"/> property reads the file and returnes the read points as lazy established <see cref="IEnumerable(Of IGeoPoint)"/>. </description></item>
     ''' <item><description> The <see cref="GeoPointFile.Store"/> method writes any given <see cref="IEnumerable(Of IGeoPoint)"/> to the file. </description></item>
     ''' <item><description> Provides the <see cref="GeoPointFile.Constraints"/> property in order to outline constraints violation in source file. </description></item>
     ''' </list>
     ''' </para>
     ''' </remarks>
    Public MustInherit Class GeoPointFile
        Inherits DataFile
        
        #Region "Private Fields"
            
            Private Shared ReadOnly Logger  As Rstyx.LoggingConsole.Logger = Rstyx.LoggingConsole.LogBox.getLogger("Rstyx.Utilities.Domain.IO.GeoPointFile")
            
        #End Region
        
        #Region "Protected Fields"
            
            ''' <summary> An internal helper for verifying point ID uniqueness. </summary>
            Protected ReadOnly IDCheckList          As New Dictionary(Of String, String)
            
            ''' <summary> A map that may assign different kind texts for output. By default it's empty. </summary>
          #  Protected ReadOnly KindTextOutputMap    As New Dictionary(Of String, String)
            
        #End Region
        
        #Region "Constuctors"
            
            ''' <summary> Creates a new instance. </summary>
            Public Sub New()
                Logger.logDebug("New(): GeoPointFile instantiated")
            End Sub
            
        #End Region
        
        #Region "Properties"
            
           ''' <summary> Determines logical constraints for the intended usage of points. Defaults to <c>None</c>. </summary>
            ''' <remarks>
            ''' <para>
            ''' If any of these contraints is violated while reading (and only while reading) the file, a <see cref="ParseError"/> will be created.
            ''' </para>
            ''' <para>
            ''' If the <see cref="IEnumerable(Of IGeoPoint)"/> returned by <see cref="GeoPointFile.PointStream"/> is intended to be converted to a <see cref="GeoPointList"/>
            ''' it's recommended to specify the <see cref="GeoPointConstraints.UniqueID"/> flag. This way ID verifying will be done
            ''' by <see cref="GeoPointFile.PointStream"/> automatically and error tracking is more verbose (incl. error display in jEdit).
            ''' </para>
            ''' </remarks>
           Public Property Constraints()    As GeoPointConstraints = GeoPointConstraints.None
            
           ''' <summary> Determines editing options for read points. Defaults to <c>None</c>. </summary>
           Public Property EditOptions()    As GeoPointEditOptions = GeoPointEditOptions.None
            
           ''' <summary> Determines output options for points to write. Defaults to <c>None</c>. </summary>
           Public Property OutputOptions()  As GeoPointOutputOptions = GeoPointOutputOptions.None
           
        #End Region
        
        #Region "Methods"
            
            ''' <summary> Reads the point file and provides the points as lazy established enumerable. </summary>
             ''' <returns> All points of <see cref="DataFile.FilePath"/> as lazy established enumerable. </returns>
             ''' <remarks>
             ''' <para>
             ''' If a file header is recognized it will be stored in <see cref="DataFile.Header"/> property before yielding the first point.
             ''' </para>
             ''' <para>
             ''' If this method fails, <see cref="GeoPointFile.ParseErrors"/> should provide the parse errors occurred."
             ''' </para>
             ''' </remarks>
             ''' <exception cref="ParseException">  At least one error occurred while parsing, hence <see cref="GeoPointFile.ParseErrors"/> isn't empty. </exception>
             ''' <exception cref="RemarkException"> Wraps any other exception. </exception>
            Public ReadOnly MustOverride Iterator Property PointStream() As IEnumerable(Of IGeoPoint)
            
            ''' <summary> Writes the points collection to the point file. </summary>
             ''' <param name="PointList"> The points to store. </param>
             ''' <remarks>
             ''' <para>
             ''' If <paramref name="PointList"/> is a <see cref="GeoPointList"/> then
             ''' it's ensured that the point ID's written to the file are unique.
             ''' Otherwise point ID's may be not unique.
             ''' </para>
             ''' </remarks>
             ''' <exception cref="ParseException">  At least one error occurred while parsing, hence <see cref="GeoPointFile.ParseErrors"/> isn't empty. </exception>
             ''' <exception cref="RemarkException"> Wraps any exception. </exception>
            Public Sub Store(PointList As IEnumerable(Of IGeoPoint))
                Me.Store(PointList, Nothing)
            End Sub
            
            ''' <summary> Writes the points collection to the point file. </summary>
             ''' <param name="PointList"> The points to store. </param>
             ''' <remarks>
             ''' <para>
             ''' If <paramref name="PointList"/> is a <see cref="GeoPointList"/> then
             ''' it's ensured that the point ID's written to the file are unique.
             ''' Otherwise point ID's may be not unique.
             ''' </para>
             ''' </remarks>
             ''' <exception cref="ParseException">  At least one error occurred while parsing, hence <see cref="GeoPointFile.ParseErrors"/> isn't empty. </exception>
             ''' <exception cref="RemarkException"> Wraps any exception. </exception>
            Public MustOverride Sub Store(PointList As IEnumerable(Of IGeoPoint), MetaData As IHeader)
            
        #End Region
        
        #Region "Protected Members"
            
            ''' <summary> Verifies that the given <paramref name="ID"/> doesn't occurs repeated since last clearing of <see cref="GeoPointFile.IDCheckList"/>. </summary>
             ''' <param name="ID"> The ID to check. </param>
             ''' <exception cref="InvalidIDException"> The given <paramref name="ID"/> does already exist. </exception>
            Protected Sub VerifyUniqueID(ID As String)
                If (IDCheckList.ContainsKey(ID)) Then
                    Throw New InvalidIDException(sprintf(Rstyx.Utilities.Resources.Messages.IDCollection_RepeatedID, ID))
                Else
                    IDCheckList.Add(ID, String.Empty)
                End If
            End Sub
            
            ''' <summary> Creates the header for the file to write. </summary>
             ''' <param name="PointList"> The point list object, that may implement <see cref="Rstyx.Utilities.IO.IHeader"/>. </param>
             ''' <param name="MetaData">  Priority over <paramref name="PointList"/>. The object to get individual header lines from if it implements <see cref="Rstyx.Utilities.IO.IHeader"/>. May be <see langword="null"/> </param>
             ''' <remarks> If <paramref name="MetaData"/> and <paramref name="PointList"/> don't provide a header, then <see cref="DataFile.Header"/> will be used. </remarks>
            Protected Overloads Function CreateFileHeader(PointList As IEnumerable(Of IGeoPoint), MetaData As IHeader) As StringBuilder
                
                Dim HeaderLines      As New StringBuilder()
                Dim IndividualHeader As IHeader = Nothing
                
                If (MetaData IsNot Nothing) Then
                    IndividualHeader = MetaData
                ElseIf (TypeOf PointList Is IHeader)
                    IndividualHeader = PointList
                End If
                
                Return Me.CreateFileHeader(IndividualHeader)
            End Function
            
            ''' <summary> Gets a log entry, that documents <see cref="GeoPointFile.EditOptions"/>. </summary>
            Protected Function GetPointEditOptionsLogText() As String
                Dim LogText As String
                LogText = sprintf(Rstyx.Utilities.Resources.Messages.GeoPointFile_EditOptions_1, Me.EditOptions.HasFlag(GeoPointEditOptions.Parse_iTC).ToDisplayString())
                If (Me.EditOptions.HasFlag(GeoPointEditOptions.ParseInfoForPointKind)) Then
                    LogText &= Rstyx.Utilities.Resources.Messages.GeoPointFile_EditOptions_2a
                ElseIf (Me.EditOptions.HasFlag(GeoPointEditOptions.ParseInfoForActualCant)) Then
                    LogText &= Rstyx.Utilities.Resources.Messages.GeoPointFile_EditOptions_2b
                Else
                    LogText &= Rstyx.Utilities.Resources.Messages.GeoPointFile_EditOptions_2c
                End If
                LogText &= sprintf(Rstyx.Utilities.Resources.Messages.GeoPointFile_EditOptions_3, Me.EditOptions.HasFlag(GeoPointEditOptions.ParseCommentToo).ToDisplayString())
                Return LogText
            End Function
            
            ''' <summary> Gets a log entry, that documents <see cref="GeoPointFile.OutputOptions"/>. </summary>
            Protected Function GetPointOutputOptionsLogText() As String
                Dim LogText As String
                LogText = sprintf(Rstyx.Utilities.Resources.Messages.GeoPointFile_OutputOptions_1, Me.OutputOptions.HasFlag(GeoPointOutputOptions.Create_iTC).ToDisplayString())
                If (Me.OutputOptions.HasFlag(GeoPointOutputOptions.CreateInfoWithPointKind)) Then
                    LogText &= Rstyx.Utilities.Resources.Messages.GeoPointFile_OutputOptions_2a
                ElseIf (Me.OutputOptions.HasFlag(GeoPointOutputOptions.CreateInfoWithActualCant)) Then
                    LogText &= Rstyx.Utilities.Resources.Messages.GeoPointFile_OutputOptions_2b
                Else
                    LogText &= Rstyx.Utilities.Resources.Messages.GeoPointFile_OutputOptions_2c
                End If
                Return LogText
            End Function
            
        #End Region
        
        #Region "Protected Overrides"
            
            ''' <summary> Restets this <see cref="DataFile"/> and re-initializes it with a new file path. </summary>
             ''' <param name="FilePath"> The complete path to the data text file to be read. </param>
            Protected Overrides Sub Reset(FilePath As String)
                MyBase.Reset(FilePath)
                Me.IDCheckList.Clear()
            End Sub
            
        #End Region
        
    End Class
    
End Namespace

' for jEdit:  :collapseFolds=3::tabSize=4::tabSize=4:
