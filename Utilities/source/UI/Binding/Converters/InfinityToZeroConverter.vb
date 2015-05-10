
Imports System
Imports System.Diagnostics
Imports System.Windows.Data
Imports System.Globalization
Imports System.Collections.Generic


Namespace UI.Binding.Converters
    
    ''' <summary> Converts between <c>Double.Infinity</c> and <c>Zero</c> - and vice versa </summary>
    <ValueConversion(GetType(Double), GetType(Double))>
    Public Class InfinityToZeroConverter
        Implements IValueConverter
        
        Public Sub New()
        End Sub
        
        ''' <summary> Converts a <c>Double.Infinity</c> to <c>Zero</c>. Other values are returned unchanged. </summary>
         ''' <param name="value">      <c>Double</c> value. </param>
         ''' <param name="targetType"> System.Type to convert to. </param>
         ''' <param name="parameter">  Ignored. </param>
         ''' <param name="culture">    Ignored. </param>
         ''' <returns>                 The input value or <c>Zero</c>. On error the input value itself is returned. </returns>
        Public Function Convert(value As Object, targetType As Type, parameter As Object, culture As System.Globalization.CultureInfo) As Object Implements IValueConverter.Convert
            Try
                If (Double.IsInfinity(CDbl(value))) Then
                    Return 0.0
                Else
                    Return value
                End If
                
            Catch ex As System.Exception
                System.Diagnostics.Trace.WriteLine(ex)
                Return value
            End Try
        End Function
        
        ''' <summary> Converts <c>Zero</c> to <c>Double.Infinity</c>. Other values are returned unchanged. </summary>
         ''' <param name="value">      <c>Double</c> value. </param>
         ''' <param name="targetType"> System.Type to convert to. </param>
         ''' <param name="parameter">  Ignored. </param>
         ''' <param name="culture">    Ignored. </param>
         ''' <returns>                 The input value or <c>Double.Infinity</c>. On error the input value itself is returned. </returns>
        Public Function ConvertBack(value As Object, targetType As Type, parameter As Object, culture As System.Globalization.CultureInfo) As Object Implements IValueConverter.ConvertBack
            Try
                If (CDbl(value).Equals(0.0)) Then
                    Return Double.PositiveInfinity
                Else
                    Return value
                End If
                
            Catch ex As System.Exception
                System.Diagnostics.Trace.WriteLine(ex)
                Return value
            End Try
        End Function
    End Class
    
End Namespace

' for jEdit:  :collapseFolds=3::tabSize=4::indentSize=4:
