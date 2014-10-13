
Imports Rstyx.Utilities.Domain
Imports Rstyx.Utilities.StringUtils

Namespace Domain
    
    ''' <summary> One single iPoint. </summary>
    Public Class GeoIPoint
        Inherits GeoPoint(Of String)
        
        #Region "Private Fields"
            
            
            
        #End Region
        
        #Region "Constuctor"
            
            ''' <summary> Creates a new, empty GeoIPoint. </summary>
            Public Sub New()
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
            
            ''' <summary> Returns a very basic output of the point. </summary>
            Public Overrides Function ToString() As String
                Return sprintf("%+21s %15.5f%15.5f%15.4f %s", Me.ID, Me.Y, Me.X, Me.Z, Me.Info)
            End Function
            
        #End Region
        
    End Class

End Namespace

' for jEdit:  :collapseFolds=3::tabSize=4::tabSize=4:
