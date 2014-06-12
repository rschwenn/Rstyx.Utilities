
Imports System
Imports System.Diagnostics
Imports System.Windows.Data
Imports System.Globalization
Imports System.Collections.Generic


Namespace UI.Binding.Converters
    
    ''' <summary> Takes a Double, applies a scale factor (or the reciprocal of a scale factor when convert back) and returns the resulting Double. </summary>
    <ValueConversion(GetType(Nullable(Of Double)), GetType(Nullable(Of Double)))>
    Public Class ScaleConverter
        Implements IValueConverter
        
        Public Sub New()
        End Sub
        
        ''' <summary> Takes a Double, applies a scale factor and returns the resulting Double. </summary>
         ''' <param name="value">      Input value. </param>
         ''' <param name="targetType"> System.Type to convert to. </param>
         ''' <param name="parameter">  The scale factor to apply. </param>
         ''' <param name="culture">    Ignored. </param>
         ''' <returns>                 The scaled input value. On error the input value itself is returned. </returns>
         ''' <remarks>                 Sample: Input=10, scale=1.3  => Return=13.0. </remarks>
        Public Function Convert(value As Object, targetType As Type, parameter As Object, culture As System.Globalization.CultureInfo) As Object Implements IValueConverter.Convert
            Try
                Dim RetValue  As Nullable(Of Double) = Nothing
                If ((value IsNot Nothing) AndAlso (parameter IsNot Nothing)) Then
                    RetValue = CDbl(value) * CDbl(parameter)
                End If
                Return RetValue
                
            Catch ex As System.Exception
                System.Diagnostics.Trace.WriteLine(ex)
                Return value
            End Try
        End Function
        
        ''' <summary> Takes a Double, applies the reciprocal of a scale factor and returns the resulting Double. </summary>
         ''' <param name="value">      Input value. </param>
         ''' <param name="targetType"> System.Type to convert to. </param>
         ''' <param name="parameter">  The scale factor to apply. </param>
         ''' <param name="culture">    Ignored. </param>
         ''' <returns>                 The scaled input value. On error the input value itself is returned. </returns>
         ''' <remarks>                 Sample: Input=10, scale=1.3  => Return=7.692307692307692. </remarks>
        Public Function ConvertBack(value As Object, targetType As Type, parameter As Object, culture As System.Globalization.CultureInfo) As Object Implements IValueConverter.ConvertBack
            Try
                Dim RetValue  As Nullable(Of Double) = Nothing
                If ((value IsNot Nothing) AndAlso (parameter IsNot Nothing)) Then
                    RetValue = CDbl(value) / CDbl(parameter)
                End If
                Return RetValue
                
            Catch ex As System.Exception
                System.Diagnostics.Trace.WriteLine(ex)
                Return value
            End Try
        End Function
    End Class
    
End Namespace

' for jEdit:  :collapseFolds=3::tabSize=4::indentSize=4:
