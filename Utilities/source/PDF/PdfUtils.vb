
Imports System
Imports System.Collections.Generic
Imports System.Linq

Imports PdfSharp.Pdf
Imports PdfSharp.Pdf.IO

Imports Rstyx.Utilities.IO
Imports Rstyx.Utilities.StringUtils
Imports Rstyx.Utilities.Collections


Namespace PDF
    
    ''' <summary> Static utility methods for dealing with PDF. </summary>
    Public NotInheritable Class PdfUtils
        
        #Region "Private Fields"
            
            Private Shared Logger As Rstyx.LoggingConsole.Logger = Rstyx.LoggingConsole.LogBox.getLogger("Rstyx.Utilities.PDF")
            
        #End Region
        
        #Region "Constructor"
            
            Private Sub New
                'Hides the Constructor
            End Sub
            
        #End Region
        
        #Region "Static Methods"
            
            ''' <summary> Joins some given PDF files into one single PDF file. </summary>
             ''' <param name="InputPaths">  List of PDF file paths to join. </param>
             ''' <param name="OutputPath">  The path for the resulting PDF file. </param>
             ''' <param name="Order">       If <see langword="true"/>, the input files will be joined ordered by file name. </param>
             ''' <param name="DeleteInput"> If <see langword="true"/>, the input files will be deleted after joining. </param>
             ''' <returns> The newly created <see cref="PdfDocument"/>. </returns>
             ''' <remarks>
             ''' Every page of created PDF file gets a related bookmark, which is the input file name.
             ''' If an input file containes more than 1 pages, the corresponding bookmarks will end with a suffix "(index)".
             ''' </remarks>
             ''' <exception cref="System.ArgumentNullException"> <paramref name="InputPaths"/> is <see langword="null"/>. </exception>
             ''' <exception cref="System.ArgumentNullException"> <paramref name="OutputPath"/> is <see langword="null"/>. </exception>
             ''' <exception cref="System.ArgumentException">     <paramref name="InputPaths"/> is empty. </exception>
             ''' <exception cref="System.InvalidOperationException"> One of the input PDF files is not a valid PDF document. </exception>
            Public Shared Function JoinPdfFiles(InputPaths As IEnumerable(Of String), OutputPath As String, Order As Boolean, DeleteInput As Boolean) As PdfDocument
                    
                If (InputPaths Is Nothing) Then Throw New System.ArgumentNullException("InputPaths")
                If (OutputPath Is Nothing) Then Throw New System.ArgumentNullException("OutputPath")
                
                Dim OutputPDF As PdfDocument = New PdfDocument()
                
                For Each FilePath As String In If(Order, InputPaths.OrderBy(Function(ByVal PathName) PathName, New AlphanumericComparer(IgnoreCase:=True)), InputPaths)
                    
                    ' Open the document to import pages from it.
                    Try
                        Dim InputPDF As PdfDocument = PdfReader.Open(FilePath, PdfDocumentOpenMode.Import)
                        Dim FileName As String      = Rstyx.Utilities.IO.FileUtils.getFilePart(FilePath, FileUtils.FilePart.Base)
                        
                        ' Iterate pages.
                        For idx As Integer = 0 To InputPDF.PageCount - 1
                            
                            Dim Page         As PdfPage     = OutputPDF.AddPage(InputPDF.Pages(idx))
                            Dim BookMarkName As String      = If(InputPDF.PageCount = 1, FileName, FileName & " (" & CStr(idx + 1) & ")")
                            Dim Outline      As PdfOutline  = OutputPDF.Outlines.Add(BookMarkName, Page)
                        Next
                        InputPDF.Close()
                        
                    'Catch ex As InvalidOperationException When ex.Message.Contains("not a valid PDF")
                    Catch ex As Exception
                        Throw New InvalidOperationException(sprintf(Rstyx.Utilities.Resources.Messages.PdfUtils_InvalidPdfDocument, FilePath), ex)
                    End Try
                Next
                
                If (OutputPDF.PageCount < 1) Then Throw New ArgumentException(sprintf(Rstyx.Utilities.Resources.Messages.PdfUtils_JoinFailedNoFiles, OutputPath))
                
                ' Save joined PDF as file.
                 OutputPDF.Save(OutputPath)
                
                ' Optionally delete input files (if joining has been successful).
                If (DeleteInput) Then
                    For Each FilePath As String In InputPaths
                        If (Not (FilePath.ToLower() = OutputPath.ToLower())) Then
                            Try
                                System.IO.File.Delete(FilePath)
                            Catch ex As Exception
                                Logger.logWarning(sprintf(Rstyx.Utilities.Resources.Messages.PdfUtils_FailedDeletePDF, FilePath))
                            End Try
                        End If
                    Next
                End If
                
                Return OutputPDF
            End Function
            
        #End Region
        
    End Class
    
End Namespace

' for jEdit:  :collapseFolds=2::tabSize=4::indentSize=4:
