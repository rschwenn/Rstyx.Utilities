
Imports System
Imports System.Math

Namespace Math
    
    ''' <summary> One single mathematical point. </summary>
     ''' <remarks>  </remarks>
    Public Class Point
        
        #Region "Constuctor"
            
            ''' <summary> Creates a new, empty Point. </summary>
            Public Sub New()
            End Sub
            
        #End Region
        
        #Region "Shared Properties"
            
            ''' <summary> The Resolution of <see cref="Point"/>'s. Defaults to <c>Double.NaN</c>. </summary>
             ''' <remarks> This value is considered by comparation operators. </remarks>
            Public Shared Resolution As Double = 0.0005
            
        #End Region
        
        #Region "Properties"
            
            ''' <summary> X coordinate. Defaults to <c>Double.NaN</c>. </summary>
            Public X As Double = Double.NaN
            
            ''' <summary> Y coordinate. Defaults to <c>Double.NaN</c>. </summary>
            Public Y As Double = Double.NaN
            
            ''' <summary> Z coordinate. Defaults to <c>Double.NaN</c>. </summary>
            Public Z As Double = Double.NaN
            
        #End Region
        
        #Region "Methods"
            
            ''' <summary> Calculates the perpendicular distance to a line in XY plane. </summary>
             ''' <param name="LineStart"> Start point of target line. </param>
             ''' <param name="LineEnd">   End point of target line. </param>
             ''' <returns> The distance to the line, if the foot point is between <paramref name="LineStart"/> and <paramref name="LineEnd"/>, otherwise <c>Double.NaN</c>. </returns>
             ''' <remarks></remarks>
             ''' <exception cref="System.ArgumentNullException"> <paramref name="Pt"/> is <see langword="null"/>. </exception>
            Public Function GetDistanceToLineXY(LineStart As Point, LineEnd As Point) As Double
                Dim Distance As Double = Double.NaN
                
                ' see http://paulbourke.net/geometry/pointlineplane/
                ' see http://forums.codeguru.com/showthread.php?194400-Distance-between-point-and-line-segment
                
                If (Not LineStart.EqualsXY(LineEnd)) Then
                    
                    Dim LineLength      As Double = LineStart.GetDistanceToPointXY(LineEnd)
                    Dim FootPointPos    As Double = ( ((Me.X - LineStart.X) * (LineEnd.X - LineStart.X)) + ((Me.Y - LineStart.Y) * (LineEnd.Y - LineStart.Y)) ) / (LineLength ^ 2)
                    
                    If (FootPointPos.IsBetween(0, 1)) Then
                        
                        Dim FootPointX As Double = Interpol(LineStart.X, LineEnd.X, FootPointPos)
                        Dim FootPointY As Double = Interpol(LineStart.Y, LineEnd.Y, FootPointPos)
                        
                        Distance = Sqrt( ((FootPointY - Me.Y) ^ 2) + ((FootPointX - Me.X) ^ 2))
                    End If
                End If
                
                Return Distance
            End Function
            
            ''' <summary> Calculates the distance to another point. </summary>
             ''' <param name="Pt"> The target point. </param>
             ''' <returns> Spatial distance to <paramref name="Pt"/>. </returns>
             ''' <exception cref="System.ArgumentNullException"> <paramref name="Pt"/> is <see langword="null"/>. </exception>
            Public Function GetDistanceToPoint(Pt As Point) As Double
                
                If (Pt Is Nothing) Then Throw New System.ArgumentNullException("Pt")
                
                Return Sqrt( ((Me.Y - Pt.Y) ^ 2) + ((Me.X - Pt.X) ^ 2) + ((Me.Z - Pt.Z) ^ 2))
            End Function
            
            ''' <summary> Calculates the distance to another point in XY plane. </summary>
             ''' <param name="Pt"> The target point. </param>
             ''' <returns> Distance to <paramref name="Pt"/> in XY plane. </returns>
             ''' <exception cref="System.ArgumentNullException"> <paramref name="Pt"/> is <see langword="null"/>. </exception>
            Public Function GetDistanceToPointXY(Pt As Point) As Double
                
                If (Pt Is Nothing) Then Throw New System.ArgumentNullException("Pt")
                
                Return Sqrt( ((Me.Y - Pt.Y) ^ 2) + ((Me.X - Pt.X) ^ 2))
            End Function
            
            ''' <summary> Checks this point for equality against a given point considering a given tolerance for each coordinate. </summary>
             ''' <param name="Pt">        Point to compare with. </param>
             ''' <param name="Tolerance"> Tolerance in [m] for each coordinate. </param>
             ''' <remarks>
             ''' The "=" and "&lt;&gt;" operators consider the static <see cref="Point.Resolution"/> value as tolerance.
             ''' This method is intended to use another tolerance.
             ''' </remarks>
             ''' <returns> <see langword="true"/> if this Point equals <paramref name="ComparePoint"/>, otherwise <see langword="false"/>. </returns>
             ''' <exception cref="System.ArgumentNullException"> <paramref name="Pt"/> is <see langword="null"/>. </exception>
            Public Function EqualsTolerance(Pt As Point, Tolerance As Double) As Boolean
                
                If (Pt Is Nothing) Then Throw New System.ArgumentNullException("Pt")
                
                Return (Me.X.EqualsTolerance(Pt.X, Tolerance) AndAlso Me.Y.EqualsTolerance(Pt.Y, Tolerance) AndAlso Me.Z.EqualsTolerance(Pt.Z, Tolerance))
            End Function
            
            ''' <summary> Checks two points for equality in XY plane. <see cref=" Point.Resolution"/> is considered as tolerance for each coordinate. </summary>
             ''' <param name="Pt"> Point to compare with. </param>
             ''' <returns> <see langword="true"/> if this Point equals <paramref name="ComparePoint"/>, otherwise <see langword="false"/>. </returns>
             ''' <exception cref="System.ArgumentNullException"> <paramref name="Pt"/> is <see langword="null"/>. </exception>
            Public Function EqualsXY(Pt As Point) As Boolean
                
                If (Pt Is Nothing) Then Throw New System.ArgumentNullException("Pt")
                
                Return (Me.X.EqualsTolerance(Pt.X, Point.Resolution) AndAlso Me.Y.EqualsTolerance(Pt.Y, Point.Resolution))
            End Function
            
            ''' <summary> Checks two points for equality in XY plane. <paramref name="Tolerance"/> is considered as tolerance for each coordinate. </summary>
             ''' <param name="Pt">        Point to compare with. </param>
             ''' <param name="Tolerance"> Tolerance in [m] for each coordinate. </param>
             ''' <returns> <see langword="true"/> if this Point equals <paramref name="ComparePoint"/>, otherwise <see langword="false"/>. </returns>
             ''' <exception cref="System.ArgumentNullException"> <paramref name="Pt"/> is <see langword="null"/>. </exception>
            Public Function EqualsXY(Pt As Point, Tolerance As Double) As Boolean
                
                If (Pt Is Nothing) Then Throw New System.ArgumentNullException("Pt")
                
                Return (Me.X.EqualsTolerance(Pt.X, Tolerance) AndAlso Me.Y.EqualsTolerance(Pt.Y, Tolerance))
            End Function
            
        #End Region

        #Region "Operators"
            
            ''' <summary> Calculates the sum of two <see cref="Point"/>'as vectors. </summary>
             ''' <param name="P1"> The first operand. </param>
             ''' <param name="P2"> The second operand. </param>
             ''' <returns> A new <see cref="Point"/> which represents the sum of the operands treated as vectors. </returns>
             ''' <exception cref="System.ArgumentNullException"> <paramref name="P1"/> or <paramref name="P2"/> is <see langword="null"/>. </exception>
            Public Shared Overloads Operator + (ByVal P1 As Point, ByVal P2 As Point) As Point
                
                If (P1 Is Nothing) Then Throw New System.ArgumentNullException("P1")
                If (P2 Is Nothing) Then Throw New System.ArgumentNullException("P2")
                
                Return New Point() With {.X = P1.X + P2.X, .Y = P1.Y + P2.Y, .Z = P1.Z + P2.Z}
            End Operator
            
            ''' <summary> Calculates the difference of two <see cref="Point"/>'as vectors. </summary>
             ''' <param name="P1"> The first operand. </param>
             ''' <param name="P2"> The second operand. </param>
             ''' <returns> A new <see cref="Point"/> which represents the difference of the operands treated as vectors. </returns>
             ''' <exception cref="System.ArgumentNullException"> <paramref name="P1"/> or <paramref name="P2"/> is <see langword="null"/>. </exception>
            Public Shared Overloads Operator - (ByVal P1 As Point, ByVal P2 As Point) As Point
                
                If (P1 Is Nothing) Then Throw New System.ArgumentNullException("P1")
                If (P2 Is Nothing) Then Throw New System.ArgumentNullException("P2")
                
                Return New Point() With {.X = P1.X - P2.X, .Y = P1.Y - P2.Y, .Z = P1.Z - P2.Z}
            End Operator
            
            ''' <summary> Checks two points for equality. <see cref=" Point.Resolution"/> is considered as tolerance for each coordinate. </summary>
             ''' <param name="P1"> The first operand. </param>
             ''' <param name="P2"> The second operand. </param>
             ''' <returns> <see langword="true"/> if <paramref name="P1"/> equals <paramref name="P2"/>, otherwise <see langword="false"/>. </returns>
             ''' <exception cref="System.ArgumentNullException"> <paramref name="P1"/> or <paramref name="P2"/> is <see langword="null"/>. </exception>
            Public Shared Overloads Operator = (ByVal P1 As Point, ByVal P2 As Point) As Boolean
                
                If (P1 Is Nothing) Then Throw New System.ArgumentNullException("P1")
                If (P2 Is Nothing) Then Throw New System.ArgumentNullException("P2")
                
                Return ( P1.X.EqualsTolerance(P2.X, Point.Resolution) AndAlso P1.Y.EqualsTolerance(P2.Y, Point.Resolution) AndAlso P1.Z.EqualsTolerance(P2.Z, Point.Resolution) )
            End Operator
            
            ''' <summary> Checks two points for unequality. <see cref=" Point.Resolution"/> is considered as tolerance for each coordinate. </summary>
             ''' <param name="P1"> The first operand. </param>
             ''' <param name="P2"> The second operand. </param>
             ''' <returns> <see langword="true"/> if <paramref name="P1"/> doesn't equal <paramref name="P2"/>, otherwise <see langword="false"/>. </returns>
             ''' <exception cref="System.ArgumentNullException"> <paramref name="P1"/> or <paramref name="P2"/> is <see langword="null"/>. </exception>
            Public Shared Overloads Operator <> (ByVal P1 As Point, ByVal P2 As Point) As Boolean
                
                If (P1 Is Nothing) Then Throw New System.ArgumentNullException("P1")
                If (P2 Is Nothing) Then Throw New System.ArgumentNullException("P2")
                
                Return ( Not (P1 = P2) )
            End Operator
            
        #End Region

        #Region "Private Methods"
            
            Private Shared Function Interpol(ValueA As Double, ValueB As Double, Ratio As Double) As Double
                Return ValueA + (ValueB - ValueA) * Ratio
            End Function

        #End Region

    End Class

End Namespace

' for jEdit:  :collapseFolds=2::tabSize=4::indentSize=4:
