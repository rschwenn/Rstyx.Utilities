
Imports System
Imports System.Diagnostics
Imports System.Windows.Data
Imports System.Globalization
Imports System.Collections.Generic


Namespace UI.Binding.Converters
    
    ''' <summary> Converts between Double.NaN and <see langword="null"/> or empty string - and vice versa </summary>
    <ValueConversion(GetType(Double), GetType(String))>
    Public Class NanToNullConverter
        Implements IValueConverter
        
        Public Sub New()
        End Sub
        
        ''' <summary> Converts a <c>Double</c> to a Checkbox state resp. <c>String</c>. </summary>
         ''' <param name="value">      <c>Double</c> value. </param>
         ''' <param name="targetType"> System.Type to convert to. </param>
         ''' <param name="parameter">  Ignored. </param>
         ''' <param name="culture">    Ignored. </param>
         ''' <returns>                 <c>String</c>: True or False. On error the input value itself is returned. </returns>
        Public Function Convert(value As Object, targetType As Type, parameter As Object, culture As System.Globalization.CultureInfo) As Object Implements IValueConverter.Convert
            Try
                If (Double.IsNaN(CDbl(value))) Then
                    Return Nothing
                Else
                    Return value
                End If
                
            Catch ex As System.Exception
                System.Diagnostics.Trace.WriteLine(ex)
                Return value
            End Try
        End Function
        
        ''' <summary> Converts a Checkbox state resp. <c>String</c> to a <c>Double</c>. </summary>
         ''' <param name="value">      <c>String</c> value. </param>
         ''' <param name="targetType"> System.Type to convert to. </param>
         ''' <param name="parameter">  Ignored. </param>
         ''' <param name="culture">    Ignored. </param>
         ''' <returns>                 <c>Double</c>: <see langword="true"/> if input value is <see langword="true"/>, otherwise <see langword="false"/>. On error the input value itself is returned. </returns>
        Public Function ConvertBack(value As Object, targetType As Type, parameter As Object, culture As System.Globalization.CultureInfo) As Object Implements IValueConverter.ConvertBack
            Try
                If (CStr(value).IsEmptyOrWhiteSpace()) Then
                    Return Double.NaN
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
