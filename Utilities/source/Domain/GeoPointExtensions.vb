
Imports System
Imports System.Collections.Generic
Imports System.Collections.ObjectModel

Imports Rstyx.Utilities.Collections
Imports Rstyx.Utilities.IO
Imports Rstyx.Utilities.StringUtils

Namespace Domain
    
    ''' <summary> Extension methods for <see cref="IEnumerable(Of IGeoPoint)"/>. </summary>
    Public Module GeoPointExtensions
        
        Private Logger As Rstyx.LoggingConsole.Logger = Rstyx.LoggingConsole.LogBox.getLogger("Rstyx.Utilities.Domain.GeoPointExtensions")

        ''' <summary> Creates a <see cref="GeoPointList"/> based on any <see cref="IEnumerable(Of IGeoPoint)"/>. </summary>
         ''' <param name="SourcePointList"> The source point list. </param>
         ''' <returns></returns>
         ''' <remarks>
         ''' If <paramref name="SourcePointList"/> is already a <see cref="GeoPointList"/>, then the same instance will be returned.
         ''' Otherwise a new instance of <see cref="GeoVEPoint"/> will be created.
         ''' </remarks>
         ''' <exception cref="InvalidIDException"> There are at least two <paramref name="SourcePoint"/>'s with same <see cref="IGeoPoint.ID"/>. </exception>
        <System.Runtime.CompilerServices.Extension()> 
        Public Function AsGeoPointList(SourcePointList As IEnumerable(Of IGeoPoint)) As GeoPointList
            If (SourcePointList Is Nothing) Then
                Return Nothing
            ElseIf (TypeOf SourcePointList Is GeoPointList) Then
                Return SourcePointList
            Else
                Return New GeoPointList(SourcePointList)
            End If
        End Function
         
         ''' <summary> Changes point ID's of <see cref="IEnumerable(Of IGeoPoint)"/> according to a ID change table. </summary>
          ''' <param name="IDChangeTab"> Table with Point pairs (source => target). </param>
          ''' <remarks></remarks>
          ''' <exception cref="System.ArgumentNullException"> <paramref name="PointList"/> is <see langword="null"/>. </exception>
          ''' <exception cref="System.ArgumentNullException"> <paramref name="IDChangeTab"/> is <see langword="null"/>. </exception>
        <System.Runtime.CompilerServices.Extension()> 
         Public Sub ChangePointNumbers(PointList As IEnumerable(Of IGeoPoint), IDChangeTab As Dictionary(Of String, String))
             
             If (PointList Is Nothing)   Then Throw New System.ArgumentNullException("PointList")
             If (IDChangeTab Is Nothing) Then Throw New System.ArgumentNullException("PointChangeTab")
             
             Dim ChangeCount As Long = 0
             
             If (IDChangeTab.Count < 1) then
                 Logger.logWarning(Rstyx.Utilities.Resources.Messages.GeoPointList_EmptyPointChangeTab)
             Else
                 For Each Point As IGeoPoint In PointList
                     
                     If (IDChangeTab.ContainsKey(Point.ID)) Then
                         Point.ID = IDChangeTab(Point.ID)
                         ChangeCount += 1
                     End If
                 Next
                 Logger.logInfo(sprintf(Rstyx.Utilities.Resources.Messages.GeoPointList_ChangePointNumbersSuccess, ChangeCount))
             End If
         End Sub
        
    End Module
    
End Namespace

' for jEdit:  :collapseFolds=3::tabSize=4::tabSize=4:
