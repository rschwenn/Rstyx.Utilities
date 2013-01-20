
Imports System
Imports System.Diagnostics
Imports System.Windows.Data
Imports System.Globalization
Imports System.Collections.Generic


Namespace UI.Binding.Converters
    
    ''' <summary> Converts between Enum property and Integer - and vice versa </summary>
    <ValueConversion(GetType(Object), GetType(Integer))>
    Public Class EnumIntConverter
        Implements IValueConverter
        
        Public Sub New()
        End Sub
        
        ''' <summary> Converts an Enum to an Integer. </summary>
         ''' <param name="value">      Enum value. </param>
         ''' <param name="targetType"> System.Type to convert to. </param>
         ''' <param name="parameter">  Ignored. </param>
         ''' <param name="culture">    Ignored. </param>
         ''' <returns>                 The underlying Enum Integer (0 on error). </returns>
        Public Function Convert(value As Object, targetType As Type, parameter As Object, culture As System.Globalization.CultureInfo) As Object Implements IValueConverter.Convert
            ' Enum => Integer
            Dim RetValue  As Integer = 0
            Try
                RetValue = value
            Catch ex As System.Exception 
                ' Silently catch
                System.Diagnostics.Debug.Print("EnumIntConverter.Convert(): Exception!")
            End Try 
            Return RetValue
        End Function
        
        ''' <summary> Converts an Integer to an Enum. </summary>
         ''' <param name="value">      Input value. </param>
         ''' <param name="targetType"> System.Type to convert to. </param>
         ''' <param name="parameter">  Ignored. </param>
         ''' <param name="culture">    Ignored. </param>
         ''' <returns>                 The matching Enum value (default value on error). </returns>
        Public Function ConvertBack(value As Object, targetType As Type, parameter As Object, culture As System.Globalization.CultureInfo) As Object Implements IValueConverter.ConvertBack
            Dim RetValue  As Object = System.Activator.CreateInstance(targetType)
            Try
                RetValue = System.Enum.ToObject(targetType, value)
            Catch ex As System.Exception 
                ' Silently catch
                System.Diagnostics.Debug.Print("EnumIntConverter.ConvertBack(): Exception!")
            End Try
            Return RetValue
        End Function
    End Class
    
End Namespace

' for jEdit:  :collapseFolds=3::tabSize=4::indentSize=4:
