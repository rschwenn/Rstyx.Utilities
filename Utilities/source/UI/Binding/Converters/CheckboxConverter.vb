
Imports System
Imports System.Diagnostics
Imports System.Windows.Data
Imports System.Globalization
Imports System.Collections.Generic


Namespace UI.Binding.Converters
    
    ''' <summary> Converts between Boolean property and Checkbox state - and vice versa </summary>
    <ValueConversion(GetType(Boolean), GetType(Nullable(Of Boolean)))>
    Public Class CheckboxConverter
        Implements IValueConverter
        
        Public Sub New()
        End Sub
        
        ''' <summary> Converts a <c>Boolean</c> to a Checkbox state resp. <c>Nullable(Of Boolean)</c>. </summary>
         ''' <param name="value">      <c>Boolean</c> value. </param>
         ''' <param name="targetType"> System.Type to convert to. </param>
         ''' <param name="parameter">  Ignored. </param>
         ''' <param name="culture">    Ignored. </param>
         ''' <returns>                 <c>Nullable(Of Boolean)</c>: True or False. On error the input value itself is returned. </returns>
        Public Function Convert(value As Object, targetType As Type, parameter As Object, culture As System.Globalization.CultureInfo) As Object Implements IValueConverter.Convert
            Try
                Return New Nullable(Of Boolean)(CBool(value))
                
            Catch ex As System.Exception
                System.Diagnostics.Trace.WriteLine(ex)
                Return value
            End Try
        End Function
        
        ''' <summary> Converts a Checkbox state resp. <c>Nullable(Of Boolean)</c> to a <c>Boolean</c>. </summary>
         ''' <param name="value">      <c>Nullable(Of Boolean)</c> value. </param>
         ''' <param name="targetType"> System.Type to convert to. </param>
         ''' <param name="parameter">  Ignored. </param>
         ''' <param name="culture">    Ignored. </param>
         ''' <returns>                 <c>Boolean</c>: <see langword="true"/> if input value is <see langword="true"/>, otherwise <see langword="false"/>. On error the input value itself is returned. </returns>
        Public Function ConvertBack(value As Object, targetType As Type, parameter As Object, culture As System.Globalization.CultureInfo) As Object Implements IValueConverter.ConvertBack
            Try
                Dim InputValue As Nullable(Of Boolean) = CType(value, Nullable(Of Boolean))
                Dim RetValue   As Nullable(Of Boolean) = False
                If (InputValue.HasValue) Then
                    RetValue = InputValue.Value
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
