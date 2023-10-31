
'Namespace Arrays
    
    ''' <summary> Static utility methods for dealing with arrays. </summary>
    Public NotInheritable Class ArrayUtils
        
        #Region "Private Fields"
            
            'Private Logger As Rstyx.LoggingConsole.Logger = Rstyx.LoggingConsole.LogBox.GetLogger(MyClass.GetType.FullName)
            Private Shared ReadOnly Logger As Rstyx.LoggingConsole.Logger = Rstyx.LoggingConsole.LogBox.GetLogger("Rstyx.Utilities.ArrayUtils")
            
        #End Region
        
        #Region "Constructor"
            
            Private Sub New
                'Hides the Constructor
            End Sub
            
        #End Region
        
        #Region "Enums"
            
            ''' <summary> Sorting types supported by sorting methods of <see cref="ArrayUtils" /> class. </summary>
            Public Enum SortType As Integer
                
                ''' <summary> Numeric if possible, otherwise alphanumeric </summary>
                Numeric      = 0
                
                ''' <summary> Alphanumeric </summary>
                Alphanumeric = 1
            End Enum
            
        #End Region
        
        #Region "Public Static Methods"
            
            ''' <summary> Unwinds a one-staged encapsulated Array into a flat Array. </summary>
             ''' <param name="WrapArray"> 1d-Array, which elements can be 1d-Arrays. </param>
             ''' <returns>
             ''' A flat 1d-Array (provides the input encapsulation has been one-staged only).
             ''' If an error occures, the input Array is returned. 
             ''' </returns>
             ''' <exception cref="System.ArgumentNullException"> <paramref name="WrapArray"/> is <see langword="null"/> or empty. </exception>
            Public Shared Function GetFlatArray(byRef WrapArray As Object()) As Object()
                
                If (WrapArray Is Nothing) Then Throw New System.ArgumentNullException("WrapArray")
                
                'Deklarationen
                  Dim i            As Integer
                  Dim j            As Integer
                  Dim k            As Integer
                  Dim ub1          As Integer
                  Dim ub2          As Integer
                  Dim CountParms1  As Integer
                  Dim CountParms2  As Integer
                  Dim EmbedCount() As Integer
                  Dim IsEmbedded   As Boolean
                  Dim TmpArray()   As Object ' = New Object() {}
                  Dim FlatParms()  As Object ' = New Object() {}
                  
                'Try
                    ' Initialisierung
                      ub1 = UBound(WrapArray)
                      CountParms1 = ub1 + 1
                      CountParms2 = 0
                      ReDim EmbedCount(ub1)
                    
                    ' Feststellen, ob und an welchen Stellen das Eingabe-Array verschachtelte Parameter enthält
                    For i = 0 To ub1
                        If (IsArray(WrapArray(i))) Then
                            IsEmbedded = True
                            If (WrapArray(i) IsNot Nothing) Then
                                ub2 = UBound(CType(WrapArray(i), System.Array))
                                EmbedCount(i) = ub2 + 1
                            Else
                                EmbedCount(i) = 0
                            End If
                        Else
                            EmbedCount(i) = 1
                        End If
                        CountParms2 += EmbedCount(i)
                    Next
                    
                    ' Ggf. das übernommene ParamArray in ein nicht verschachteltes Array wandeln
                    If (Not IsEmbedded) Then
                        flatParms = WrapArray
                    Else
                        ReDim FlatParms(CountParms2 - 1)
                        j = 0
                        For i = 0 To ub1
                            If (EmbedCount(i) > 0) Then
                                If (IsArray(WrapArray(i))) Then
                                    tmpArray = CType(WrapArray(i), Object())
                                    For k = 0 To UBound(CType(tmpArray, System.Array))
                                      FlatParms(j) = TmpArray(k)
                                      j += 1
                                    Next
                                Else
                                    FlatParms(j) = WrapArray(i)
                                    j += 1
                                End If
                            End If
                        Next
                    End If
                    tmpArray = flatParms
                    
                'Catch ex As System.Exception
                '    Logger.LogError(ex, "getFlatArray(): unbekannter Fehler")
                '    tmpArray = WrapArray
                'End Try
                
                Return tmpArray
            End Function
            
            ''' <summary> Sorts the the whole 2d-Array "Matrix" by a given column by invoking <see cref="ArrayUtils.QuickSort2d" />. </summary>
             ''' <param name="Matrix">      The Array to sort. </param>
             ''' <param name="Key_Dim">     Determines the column to sort by (in conjunction with "Key_Idx"). </param>
             ''' <param name="Key_Idx">     Determines the column to sort by (in conjunction with "Key_Dim"). </param>
             ''' <param name="SortingType"> Numeric or alfanumeric </param>
             ''' <param name="Descending">  If True, sorting is performed descending. </param>
             ''' <remarks></remarks>
            Public Shared Sub SortArray2d(byRef Matrix As Object(,), byVal Key_Dim As Integer, byVal Key_Idx As Integer, byVal SortingType As SortType, byVal Descending As Boolean)
                Dim sec_Dim  As Integer
                
                'Try
                    If (Key_Dim = 1) Then sec_Dim = 2 Else sec_Dim = 1
                    Dim LowerIndex  As Integer = Lbound(Matrix, sec_Dim)
                    Dim UpperIndex  As Integer = Ubound(Matrix, sec_Dim)
                    Logger.LogDebug(StringUtils.Sprintf("SortArray2d(): Initialisierung QuickSort2d(Matrix,%s,%s,%s,%s,%s,%s", Key_Dim, Key_Idx, SortingType.ToDisplayString(), Descending, LowerIndex, UpperIndex))
                    QuickSort2d(Matrix, Key_Dim, Key_Idx, SortingType, Descending, LowerIndex, UpperIndex)
                    
                'Catch ex As System.Exception
                '    Logger.LogError(ex, "SortArray2d(): unbekannter Fehler")
                'End Try
            End Sub
            
            ''' <summary> Sorts a given range of a 2d-Array "Matrix" by a given column. </summary>
             ''' <param name="Matrix">      The Array to sort. </param>
             ''' <param name="Key_Dim">     Determines the column to sort by (in conjunction with "Key_Idx"). </param>
             ''' <param name="Key_Idx">     Determines the column to sort by (in conjunction with "Key_Dim"). </param>
             ''' <param name="SortingType"> Numeric or alfanumeric </param>
             ''' <param name="Descending">  If True, sorting is performed descending. </param>
             ''' <param name="LowerIndex">  Start of range to sort (for QuickSort internal partitioning). </param>
             ''' <param name="UpperIndex">  End of range to sort (for QuickSort internal partitioning). </param>
             ''' <remarks> 
             ''' <para>
             ''' To sort the entire array <see cref="ArrayUtils.SortArray2d" /> should be used which in turn utilizes this method. 
             ''' </para>
             ''' <para>
             ''' Based on a <a href="http://www.activevb.de/tutorials/tut_sortalgo/sortalgo.html" > tutorial for 1d-Array QuickSort by Klaus Neumann </a>
             ''' </para>
             ''' </remarks>
            Public Shared Sub QuickSort2d(byRef Matrix As Object(,), _
                                          byVal Key_Dim As Integer, _
                                          byVal Key_Idx As Integer, _ 
                                          byVal SortingType As SortType, _
                                          byVal Descending As Boolean, _
                                          byVal LowerIndex As Integer, _
                                          byVal UpperIndex As Integer)
                'Dim sec_Dim     As Long
                Dim i           As Integer
                Dim k           As Integer
                Dim idxM        As Integer
                Dim idxColumn   As Integer
                Dim Value_idxM  As Object
                Dim temp        As Object
                
                'Try
                    'Logger.LogDebug("QuickSort2d(): Starte QuickSort2d(Matrix," & Key_Dim & "," & Key_Idx & "," & SortingType & "," & Descending & "," & LowerIndex & "," & UpperIndex & ")") 
                    If ((LowerIndex < 0) or (UpperIndex < 0)) Then
                        Logger.LogDebug("QuickSort2d(): Sortieren unmöglich, da mindestens ein Index < 0!")
                    Else
                        'If (Key_Dim = 1) Then sec_Dim = 2 Else sec_Dim = 1
                        
                        idxM = CInt(System.Math.Truncate((LowerIndex + UpperIndex) / 2))
                        i    = LowerIndex
                        k    = UpperIndex
                        
                        'Pivotelement: Wert ca. in der Mitte der Schlüsselspalte.
                        Value_idxM = ArrayValue(Matrix, Key_Dim, Key_Idx, idxM)
                        
                        Do
                            Do While IsLesser(ArrayValue(Matrix, Key_Dim, Key_Idx, i), Value_idxM, SortingType, Descending)
                                i += 1
                                'Schleifenausgang spätestens mit i = idxM + 1
                            Loop
                            
                            Do While IsLesser(Value_idxM, ArrayValue(Matrix, Key_Dim, Key_Idx, k), SortingType, Descending)
                                k -= 1
                                'Schleifenausgang spätestens mit k = idxM - 1
                            Loop
                            
                            If (i <= k) Then
                                'Arraywerte der Indizes i und k tauschen.
                                For idxColumn = Lbound(Matrix, Key_Dim) To Ubound(Matrix, Key_Dim)
                                    if (Key_Dim = 1) Then
                                        temp = Matrix(idxColumn, k)
                                        Matrix(idxColumn, k) = Matrix(idxColumn, i)
                                        Matrix(idxColumn, i) = temp
                                    Else
                                        temp = Matrix(k, idxColumn)
                                        Matrix(k, idxColumn) = Matrix(i, idxColumn)
                                        Matrix(i, idxColumn) = temp
                                    end if
                                Next
                                
                                i += 1
                                k -= 1
                            End If
                            
                        Loop Until (i > k)
                        
                        If (LowerIndex < k) Then QuickSort2d(Matrix, Key_Dim, Key_Idx, SortingType, Descending, LowerIndex, k)
                        If (i < UpperIndex) Then QuickSort2d(Matrix, Key_Dim, Key_Idx, SortingType, Descending, i, UpperIndex)
                    End If
                    
                'Catch ex As System.Exception
                '    Logger.LogError(ex, "QuickSort2d(): unbekannter Fehler")
                'End Try
            End Sub
            
        #End Region
        
        #Region "Private Members"
            
            ''' <summary> Returns the value of a given matrix element (Helper for QuickSort2d). </summary>
             ''' <param name="Matrix">  2d-Array to sort.                                                 </param>
             ''' <param name="Key_Dim"> (1|2)   No. of Dimension, containing the key column.              </param>
             ''' <param name="Key_Idx"> (0,1,.) No. of Index (in Key_Dim) which points to the key column. </param>
             ''' <param name="Sek_Idx"> (0,1,.) No. of the other Index.                                   </param>
             ''' <returns> Value of a given matrix element. </returns>
            Private Shared Function ArrayValue(byRef Matrix As Object(,), byVal Key_Dim As Integer, byVal Key_Idx As Integer, byVal Sek_Idx As Integer) As Object
                Dim RetValue  As Object
                
                If (Key_Dim = 1) Then
                    RetValue = Matrix(Key_Idx, Sek_Idx)
                Else
                    RetValue = Matrix(Sek_Idx, Key_Idx)
                End if
                
                Return RetValue
            End Function
            
            ''' <summary> Compares two object values </summary>
             ''' <param name="Value1"> First value </param>
             ''' <param name="Value2"> Second value </param>
             ''' <param name="SortingType"> Numeric or alfanumeric </param>
             ''' <param name="Reverse"> If true, the return value means "isGreater" </param>
             ''' <returns> True, if first value is lesser than second otherwise false. </returns>
             ''' <remarks> A Null value is lesser than all other values. </remarks>
            Private Shared Function IsLesser(ByRef Value1 As Object, ByRef Value2 As Object, ByVal SortingType As SortType, ByVal Reverse As Boolean) As Boolean
                Dim CompareResult     As Boolean = false
                Dim intRev            As Integer
                
                If (Reverse) Then intRev = -1 Else intRev = 1
                
                If (IsNothing(Value1) OrElse IsNothing(Value2)) Then
                    'Ein NULL-Wert ist immer kleiner als alle anderen Werte
                    If (IsNothing(Value1) And IsNothing(Value2)) Then
                        CompareResult = false
                    Elseif (IsNothing(Value1)) Then
                        If (not Reverse) Then CompareResult = true Else CompareResult = false
                    Else
                        If (Reverse) Then CompareResult = true Else CompareResult = false
                    End if
                Else
                    If (SortingType = SortType.Numeric) Then
                        'Wenn numerisch nicht möglich, dann Alphanumerisch => Texte werden ans Ende sortiert.
                        if (not (isNumeric(Value1) and IsNumeric(Value2))) Then SortingType = SortType.Alphanumeric
                    End if
                    
                    Select Case SortingType
                      
                      Case SortType.Numeric
                          Dim DblValue1 As Double = CDbl(Value1)
                          Dim DblValue2 As Double = CDbl(Value2)
                          if (not Reverse) Then CompareResult = (DblValue1 < DblValue2) Else CompareResult = (DblValue2 < DblValue1)
                          
                      Case Else  ' Alphanumeric
                          Dim String1 As String = CStr(Value1)
                          Dim String2 As String = CStr(Value2)
                          if (strComp(String1, String2, vbTextCompare) * intRev = -1) Then CompareResult = true
                    End Select
                End if
                
                Return CompareResult
            End Function
            
        #End Region
        
    End Class
    
    ' <summary> Extension methods for Arrays or other types dealing with Arrays. </summary>
    'Public Module ArrayExtensions
        
        '<System.Runtime.CompilerServices.Extension()> 
        'Public Function ToArray(ByRef Value As System.Collections.Generic.IEnumerable(Of T)) As Array(Of T)
        '    ' Convert FileFilters IEnumerable to an Array
        '    Dim i As Integer = 0
        '    For Each FileFilter As String In FileFilters
        '        FileFilterArray(++i) = FileFilter
        '        i += 1
        '    Next
        '    Return Value
        'End Function
        
        
    'End Module
    
'End Namespace

' for jEdit:  :collapseFolds=2::tabSize=4::indentSize=4:
