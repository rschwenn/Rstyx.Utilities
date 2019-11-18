
Imports System.Collections.Generic

Namespace Domain
    
    ''' <summary> Extension methods for <see cref="IEnumerable(Of IGeoPoint)"/>. </summary>
    Public Module GeoPointExtensions
        
        'Private Logger As Rstyx.LoggingConsole.Logger = Rstyx.LoggingConsole.LogBox.getLogger("Rstyx.Utilities.Domain.GeoPointExtensions")

        ''' <summary> Creates a <see cref="GeoPointList"/> based on any <see cref="IEnumerable(Of IGeoPoint)"/>. </summary>
         ''' <param name="SourcePointList"> The source point list. </param>
         ''' <returns></returns>
         ''' <remarks>
         ''' If <paramref name="SourcePointList"/> is already a <see cref="GeoPointList"/>, then the same instance will be returned.
         ''' Otherwise a new instance of <see cref="GeoPointList"/> will be created.
         ''' </remarks>
         ''' <exception cref="InvalidIDException"> There are at least two points in <paramref name="SourcePointList"/>'s with same <see cref="IGeoPoint.ID"/>. </exception>
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

        ''' <summary> Creates a <see cref="GeoPointOpenList"/> based on any <see cref="IEnumerable(Of IGeoPoint)"/>. </summary>
         ''' <param name="SourcePointList"> The source point list. </param>
         ''' <returns></returns>
         ''' <remarks>
         ''' If <paramref name="SourcePointList"/> is already a <see cref="GeoPointOpenList"/>, then the same instance will be returned.
         ''' Otherwise a new instance of <see cref="GeoPointOpenList"/> will be created.
         ''' </remarks>
        <System.Runtime.CompilerServices.Extension()> 
        Public Function AsGeoPointOpenList(SourcePointList As IEnumerable(Of IGeoPoint)) As GeoPointOpenList
            If (SourcePointList Is Nothing) Then
                Return Nothing
            ElseIf (TypeOf SourcePointList Is GeoPointOpenList) Then
                Return SourcePointList
            Else
                Return New GeoPointOpenList(SourcePointList)
            End If
        End Function
        
    End Module
    
End Namespace

' for jEdit:  :collapseFolds=3::tabSize=4::tabSize=4:
