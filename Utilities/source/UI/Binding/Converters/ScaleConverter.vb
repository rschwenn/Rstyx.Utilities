
Imports System
Imports System.Diagnostics
Imports System.Windows.Data
Imports System.Globalization
Imports System.Collections.Generic


Namespace UI.Binding.Converters
    
    ''' <summary> Takes a Double, applies a scale factor and returns the resulting Double. </summary>
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
         ''' <returns>                 The scaled input value. </returns>
         ''' <remarks>                 Sample: Input=10, scale=1.3  => Return=13.0. </remarks>
        Public Function Convert(value As Object, targetType As Type, parameter As Object, culture As System.Globalization.CultureInfo) As Object Implements IValueConverter.Convert
            Dim RetValue  As Nullable(Of Double) = Nothing
            Try
                If ((value IsNot Nothing) AndAlso (parameter IsNot Nothing)) Then
                    RetValue = CDbl(value) * CDbl(parameter)
                End If
            Catch ex As System.Exception 
                ' Silently catch
                System.Diagnostics.Debug.Print("ScaleConverter.Convert(): Exception!")
            End Try 
            Return RetValue
        End Function
        
        ''' <summary> Not Implemented. </summary>
         ''' <param name="value">      Integer value. </param>
         ''' <param name="targetType"> System.Type to convert to. </param>
         ''' <param name="parameter">  Ignored. </param>
         ''' <param name="culture">    Ignored. </param>
         ''' <returns>                 System.NotImplementedException. </returns>
        Public Function ConvertBack(value As Object, targetType As Type, parameter As Object, culture As System.Globalization.CultureInfo) As Object Implements IValueConverter.ConvertBack
            Return New System.NotImplementedException()
        End Function
    End Class
    
End Namespace

' for jEdit:  :collapseFolds=3::tabSize=4::indentSize=4:
