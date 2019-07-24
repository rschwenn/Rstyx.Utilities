
Imports System.Collections.Generic

Namespace Collections
    
    ''' <summary> A <see cref="System.Collections.Generic.IComparer(Of String)"/> that does numeric comparing for integer numbers in the strings. </summary>
     ''' <remarks> 
     ''' <para>
     ''' In contrast to standard string comparing this class says that "String 2" &lt; "String 10". 
     ''' </para>
     ''' <para>
     ''' Algorithm source: http://www.dotnetperls.com/alphanumeric-sorting
     ''' </para>
     ''' </remarks>
    Public Class AlphanumericComparer
        Implements IComparer(Of String)
        
        Private _IgnoreCase  As Boolean = True
        
        #Region "Constructors"
            
            ''' <summary> Hides the default constructor. </summary>
            Private Sub New
            End Sub
            
            ''' <summary> Creates a new instance. </summary>
             ''' <param name="IgnoreCase"> If True, strings are compared case insensitive. </param>
            Public Sub New(IgnoreCase As Boolean)
                Mybase.New()
                _IgnoreCase = IgnoreCase
            End Sub
            
        #End Region
        
        #Region "IComparer Interface"
            
            ''' <summary> Compares two strings. Numbers inside the strings are compared in a numeric way. </summary>
             ''' <param name="x"> First string </param>
             ''' <param name="y"> Second string </param>
             ''' <returns> See: <see cref="System.Collections.Generic.Comparer(Of T)"/> </returns>
            Public Function Compare(x As String, y As String) As Integer Implements IComparer(Of String).Compare
                
                Dim finished  As Boolean = False
                Dim RetValue  As Integer = 0
                
                If (Not ((x Is Nothing) OrElse (y Is Nothing))) Then
                    
                    Dim s1       As String  = x
                    Dim s2       As String  = y
                    
                    If (_IgnoreCase) Then
                        s1 = s1.ToLower()
                        s2 = s2.ToLower()
                    End If
                    
                    Dim len1     As Integer = s1.Length
                    Dim len2     As Integer = s2.Length
                    Dim marker1  As Integer = 0
                    Dim marker2  As Integer = 0
                    
                    ' Walk through the strings with two markers.
                    Do While (marker1 < len1 AndAlso marker2 < len2)
                        Dim ch1 As Char = s1(marker1)
                        Dim ch2 As Char = s2(marker2)
                        
                        ' Some buffers we can build up characters in for each chunk.
                        Dim loc1               As Integer = 0
                        Dim loc2               As Integer = 0
                        Dim space1(len1 - 1)   As Char
                        Dim space2(len2 - 1)   As Char
                        
                        ' Walk through all following characters that are digits or
                        ' characters in BOTH strings starting at the appropriate marker.
                        ' Collect char arrays.
                        Do
                            space1(loc1) = ch1
                            loc1 += 1
                            marker1 += 1
                            
                            If (marker1 < len1) Then
                                ch1 = s1(marker1)
                            Else
                                Exit Do
                            End If
                        Loop While (Char.IsDigit(ch1) = Char.IsDigit(space1(0)))
                        
                        Do
                            space2(loc2) = ch2
                            loc2 += 1
                            marker2 += 1
                            
                            If (marker2 < len2) Then
                                ch2 = s2(marker2)
                            Else
                                Exit Do
                            End If
                        Loop While (Char.IsDigit(ch2) = Char.IsDigit(space2(0)))
                        
                        ' If we have collected numbers, compare them numerically.
                        ' Otherwise, if we have strings, compare them alphabetically.
                        Dim str1    As New String(space1)
                        Dim str2    As New String(space2)
                        Dim result  As Integer
                        
                        If (Char.IsDigit(space1(0)) AndAlso Char.IsDigit(space2(0))) Then
                            'Dim thisNumericChunk As Long = Long.Parse(str1)  ' Long:  max. 19 digits.
                            'Dim thatNumericChunk As Long = Long.Parse(str2)
                            Dim thisNumericChunk As Double = Double.Parse(str1)
                            Dim thatNumericChunk As Double = Double.Parse(str2)
                            result = thisNumericChunk.CompareTo(thatNumericChunk)
                        Else
                            result = str1.CompareTo(str2)
                        End If
                        
                        If (Not (result = 0)) Then
                            'Return result
                            RetValue = result
                            finished = True
                            Exit Do
                        End If
                    Loop
                    If (Not finished) Then RetValue = len1 - len2
                End If
                
                Return RetValue
            End Function
            
        #End Region
        
    End Class
    
End Namespace

' for jEdit:  :collapseFolds=2::tabSize=4::indentSize=4:
