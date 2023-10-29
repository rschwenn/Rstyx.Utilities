
Imports System
Imports System.Diagnostics
Imports System.Windows.Data
Imports System.Globalization
Imports System.Collections.Generic


Namespace UI.Binding.Converters
    
    ''' <summary> Converts between CultureInfo and it's NativeName String - and vice versa. </summary>
     ''' <remarks> Support for single variables and List(Of CultureInfo). </remarks>
    <ValueConversion(GetType(System.Globalization.CultureInfo), GetType(String))>
    Public Class CultureInfoConverter
        Implements IValueConverter
        
        Private ReadOnly NativeName2CultureInfo  As New Dictionary(Of String, CultureInfo)
        
        Public Sub New()
        End Sub
        
        Private Function CultureInfo2Name(Culture As System.Globalization.CultureInfo) As String
            Dim NativeName  As String
            
            If (Culture.Equals(System.Globalization.CultureInfo.InvariantCulture)) Then
                NativeName = "(default)"
            Else
                NativeName = Culture.NativeName
            End If
            
            'Remember mapping of NativeName and CultureInfo
            If (Not NativeName2CultureInfo.ContainsKey(NativeName)) then
                NativeName2CultureInfo.Add(NativeName, Culture)
            End if
            
            Return NativeName
        End Function
        
        ''' <summary> Converts a CultureInfo to its NativeName String. </summary>
         ''' <param name="value">      CultureInfo value. </param>
         ''' <param name="targetType"> System.Type to convert to. </param>
         ''' <param name="parameter">  Ignored. </param>
         ''' <param name="culture">    Ignored. </param>
         ''' <returns>                 The matching NativeName String. On error the input value itself is returned. </returns>
        Public Function Convert(value As Object, targetType As Type, parameter As Object, culture As System.Globalization.CultureInfo) As Object Implements IValueConverter.Convert
            Try
                Dim ret As Object = Nothing
                
                If (value IsNot Nothing) then
                    Dim valueType As Type = value.GetType()
                    
                    If (valueType.IsGenericType) Then
                        Dim retList As New List(Of String)
                        For Each ci As CultureInfo In DirectCast(value, List(Of CultureInfo))
                            retList.Add(CultureInfo2Name(ci))
                        Next
                        ret = retList
                    Else
                        ret = CultureInfo2Name(DirectCast(value, CultureInfo))
                    End if
                End if
                Return ret
                
            Catch ex As System.Exception
                System.Diagnostics.Trace.WriteLine(ex)
                Return value
            End Try
        End Function
        
        
        ''' <summary> Converts a CultureInfo's NativeName String to a CultureInfo. </summary>
         ''' <param name="value">      CultureInfo's NativeName String value. </param>
         ''' <param name="targetType"> System.Type to convert to. </param>
         ''' <param name="parameter">  Ignored. </param>
         ''' <param name="culture">    Ignored. </param>
         ''' <returns>                 The matching CultureInfo. On error the input value itself is returned. </returns>
        Public Function ConvertBack(value As Object, targetType As Type, parameter As Object, culture As System.Globalization.CultureInfo) As Object Implements IValueConverter.ConvertBack
            Try
                Dim ret As Object = Nothing
                
                If (Not NativeName2CultureInfo.ContainsKey(CStr(value))) then
                    Trace.WriteLine(String.Format("CultureInfoConverter[ConvertBack]: Can't get CultureInfo for NativeName {0}", value))
                Else
                    ret = NativeName2CultureInfo(CStr(value))
                End if
                
                Return ret
                
            Catch ex As System.Exception
                System.Diagnostics.Trace.WriteLine(ex)
                Return value
            End Try
        End Function
    End Class
    
End Namespace

' for jEdit:  :collapseFolds=3::tabSize=4::indentSize=4:
