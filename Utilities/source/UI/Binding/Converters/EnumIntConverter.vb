
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
         ''' <returns>                 The underlying Enum Integer. On error the input value itself is returned. </returns>
        Public Function Convert(value As Object, targetType As Type, parameter As Object, culture As System.Globalization.CultureInfo) As Object Implements IValueConverter.Convert
            ' Enum => Integer
            Try
                Return CInt(value)
            Catch ex As System.Exception
                System.Diagnostics.Trace.WriteLine(ex)
                Return value
            End Try 
        End Function
        
        ''' <summary> Converts an Integer to an Enum. </summary>
         ''' <param name="value">      Input value. </param>
         ''' <param name="targetType"> System.Type to convert to. </param>
         ''' <param name="parameter">  Ignored. </param>
         ''' <param name="culture">    Ignored. </param>
         ''' <returns>                 The matching Enum value. On error the input value itself is returned. </returns>
        Public Function ConvertBack(value As Object, targetType As Type, parameter As Object, culture As System.Globalization.CultureInfo) As Object Implements IValueConverter.ConvertBack
            ' Integer => Enum
            Try
                Return System.Enum.ToObject(targetType, value)
            Catch ex As System.Exception 
                System.Diagnostics.Trace.WriteLine(ex)
                'Dim RetValue  As Object = System.Activator.CreateInstance(targetType)
                Return value
            End Try
        End Function
    End Class
    
End Namespace

' for jEdit:  :collapseFolds=3::tabSize=4::indentSize=4:
