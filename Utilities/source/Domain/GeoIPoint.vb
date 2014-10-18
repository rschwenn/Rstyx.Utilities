
Imports Rstyx.Utilities.Domain
Imports Rstyx.Utilities.IO
Imports Rstyx.Utilities.StringUtils

Namespace Domain
    
    ''' <summary> One single iPoint. </summary>
    Public Class GeoIPoint
        Inherits GeoPoint
        
        #Region "Private Fields"
            
            Private MaxIDLength     As Integer = 20
            
        #End Region
        
        #Region "Constuctor"
            
            ''' <summary> Creates a new, empty <see cref="GeoIPoint"/>. </summary>
            Public Sub New()
            End Sub
            
            ''' <summary> Creates a new <see cref="GeoIPoint"/> and inititializes it's properties from any given <see cref="GeoPoint"/>. </summary>
             ''' <param name="SourcePoint"> The source point to get init values from. May be <see langword="null"/>. </param>
             ''' <remarks></remarks>
             ''' <exception cref="ParseException"> ID of <paramref name="SourcePoint"/> isn't a valid ID for this point (The <see cref="ParseError"/> only contains a message.). </exception>
            Public Sub New(SourcePoint As GeoPoint)
                MyBase.New(SourcePoint)
            End Sub
            
        #End Region
        
        #Region "Properties"
            
            ''' <summary> Calculation Code. </summary>
            Public Property CalcCode    As String = Nothing
            
            ''' <summary> Grafics Code. </summary>
            Public Property GraficsCode As String = Nothing
            
            ''' <summary> Grafics Dimension. </summary>
            Public Property GraficsDim  As Double = Double.NaN
            
            ''' <summary> Grafics Eccentricity. </summary>
            Public Property GraficsEcc  As Double = Double.NaN
            
            ''' <summary> Type of Coordinates, i.e. "YXZ". </summary>
            Public Property CoordType   As String = Nothing
            
            ''' <summary> Flags. </summary>
            Public Property Flags       As String = Nothing
            
            ''' <summary> Key of Attribute 1. </summary>
            Public Property AttKey1     As String = Nothing
            
            ''' <summary> Value of Attribute 1. </summary>
            Public Property AttValue1   As String = Nothing
            
            ''' <summary> Key of Attribute 2. </summary>
            Public Property AttKey2     As String = Nothing
            
            ''' <summary> Value of Attribute 2. </summary>
            Public Property AttValue2   As String = Nothing
            
        #End Region
        
        #Region "Overrides"
            
            ''' <summary> Parses the given string as new <see cref="GeoPoint.ID"/> for this GeoPoint. </summary>
             ''' <param name="TargetID"> The string which is intended to become the new point ID. It will be trimmed here. </param>
             ''' <returns> The parsed ID. </returns>
             ''' <remarks>
             ''' <para>
             ''' This method will be called when setting the <see cref="GeoPoint.ID"/> property.
             ''' It allows for special format conversions and for validation of <paramref name="TargetID"/>.
             ''' </para>
             ''' </remarks>
             ''' <exception cref="InvalidIDException"> <paramref name="TargetID"/> is longer than 20 characters. </exception>
            Protected Overrides Function ParseID(TargetID As String) As String
                
                Dim RetValue As String = MyBase.ParseID(TargetID)
                
                If (RetValue.Length > MaxIDLength) Then
                    Throw New InvalidIDException(sprintf(Rstyx.Utilities.Resources.Messages.GeoPointConstraints_PointIDToLong, RetValue, MaxIDLength))
                End If
                
                Return RetValue
            End Function
            
            ''' <summary> Returns a very basic output of the point. </summary>
            Public Overrides Function ToString() As String
                Return sprintf(" %+20s %15.5f%15.5f%15.4f %s", Me.ID, Me.Y, Me.X, Me.Z, Me.Info)
            End Function
            
        #End Region
        
    End Class

End Namespace

' for jEdit:  :collapseFolds=3::tabSize=4::tabSize=4:
