
Imports System
Imports System.Math
Imports System.Collections.Generic

'Imports Rstyx.Utilities.StringUtils

Namespace Math
    
    ''' <summary> A linear function represented by an ordered number of basepoints. </summary>
     ''' <remarks>  </remarks>
    Public Class LinearTabFunction
        
        #Region "Private Fields"
            
            Private ReadOnly BasePoints As List(Of BasePoint) = New List(Of BasePoint)
            
        #End Region
        
        #Region "Constuctor"
            
            ''' <summary> Creates a new, empty LinearTabFunction. </summary>
            Public Sub New()
            End Sub
            
        #End Region
        
        #Region "Nested Members"
            
            ''' <summary> A base point of a mathematical Function. </summary>
            Public Structure BasePoint
                
                ''' <summary> X coordinate of the base point. </summary>
                Public x  As Double
                
                ''' <summary> Y coordinate of the base point (function value). </summary>
                Public Fx As Double
                
            End Structure
            
        #End Region
        
        #Region "Properties"
            
            ''' <summary> Gets the X coordinate of base point at given index. </summary>
             ''' <param name="Index"> The zero-based index of base point's table. </param>
             ''' <value> The base point's X coordinate. </value>
             ''' <returns> The base point's X coordinate. </returns>
             ''' <exception cref="System.ArgumentOutOfRangeException"> <paramref name="Index"/> doesn't exist. </exception>
            Public ReadOnly Property BaseX(Index As Integer) As Double
                Get
                    Return BasePoints(Index).x
                End Get
            End Property
            
            ''' <summary> Gets the Y coordinate (function value) of base point at given index. </summary>
             ''' <param name="Index"> The zero-based index of base point's table. </param>
             ''' <value> The base point's Y coordinate (function value). </value>
             ''' <returns> The base point's Y coordinate (function value). </returns>
             ''' <exception cref="System.ArgumentOutOfRangeException"> <paramref name="Index"/> doesn't exist. </exception>
            Public ReadOnly Property BaseFx(Index As Integer) As Double
                Get
                    Return BasePoints(Index).Fx
                End Get
            End Property
            
        #End Region
        
        #Region "Methods"
            
            ''' <summary> Adds a base point. </summary>
             ''' <param name="x">  X coordinate of base point. </param>
             ''' <param name="Fx"> Y coordinate (function value) of base point. </param>
             ''' <exception cref="System.ArgumentOutOfRangeException"> <paramref name="x"/> or <paramref name="Fx"/> is <c>Double.NaN</c>. </exception>
            Public Sub AddBasePoint(x As Double, Fx As Double)
                
                If (Double.IsNaN(x))  Then Throw New ArgumentOutOfRangeException("x is NaN")
                If (Double.IsNaN(Fx)) Then Throw New ArgumentOutOfRangeException("Fx is NaN")
                
                BasePoints.Add(New BasePoint With {.x=x, .Fx=Fx})
            End Sub
            
            ''' <summary> Evaluates an interpolated function value for <paramref name="x"/>. </summary>
             ''' <param name="x"> x value of interest. </param>
             ''' <returns> Interpolated value if possible, otherwise <c>Double.NaN</c>. </returns>
            Public Function EvaluateFx(x As Double) As Double
                
                Dim RetValue As Double = Double.NaN
                
                If (Not Double.IsNaN(x)) Then
                    
                    For i As Integer = 0 To BasePoints.Count - 2
                        
                        If ( (x >= BasePoints(i).x) AndAlso (x < BasePoints(i + 1).x) ) Then
                            
                            Dim xa  As Double = BasePoints(i).x
                            Dim Fxa As Double = BasePoints(i).Fx
                            
                            RetValue = Fxa + ((BasePoints(i + 1).Fx - Fxa)  *  (x - xa) / (BasePoints(i + 1).x - xa))
                        Exit For
                        End If
                    Next
                End If
                
                Return RetValue
            End Function
            
        #End Region
        
        #Region "Overrides"
            
            ' ''' <inheritdoc/>
            ' Public Overrides Function ToString() As String
            '     Return sprintf("%13.4f, %13.4f, %13.4f", Me.X, Me.Y, Me.Z)
            ' End Function
            
        #End Region
        
        #Region "Private Methods"
            
            
        #End Region
        
    End Class
    
End Namespace

' for jEdit:  :collapseFolds=2::tabSize=4::indentSize=4:
