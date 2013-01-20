
Imports System
Imports System.Diagnostics
Imports System.Windows.Data
Imports System.Globalization
Imports System.Collections.Generic


Namespace UI.Binding.Converters
    
    ''' <summary> Converts between CultureInfo and its NativeName String - and vice versa. </summary>
     ''' <remarks> Support for single variables and List(Of CultureInfo). </remarks>
    <ValueConversion(GetType(System.Globalization.CultureInfo), GetType(String))>
    Public Class CultureInfoConverter
        Implements IValueConverter
        
        Private NativeName2CultureInfo  As New Dictionary(Of String, CultureInfo)
        
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
            if (not NativeName2CultureInfo.ContainsKey(NativeName)) then
                NativeName2CultureInfo.Add(NativeName, Culture)
            end if
            
            Return NativeName
        End Function
        
        ''' <summary> Converts a CultureInfo to its NativeName String. </summary>
         ''' <param name="value">      CultureInfo value. </param>
         ''' <param name="targetType"> System.Type to convert to. </param>
         ''' <param name="parameter">  Ignored. </param>
         ''' <param name="culture">    Ignored. </param>
         ''' <returns>                 The matching NativeName String. </returns>
        Public Function Convert(value As Object, targetType As Type, parameter As Object, culture As System.Globalization.CultureInfo) As Object Implements IValueConverter.Convert
            Dim ret As Object = Nothing
            
            if (value isNot Nothing) then
                Dim valueType   As Type = value.GetType()
                
                if (valueType.IsGenericType) then
                    dim retList As New List(Of String)
                    for each ci as CultureInfo in value
                        retList.add(CultureInfo2Name(ci))
                    next
                    ret = retList
                else
                    ret = CultureInfo2Name(value)
                end if
            end if
            Return ret
        End Function
        
        
        ''' <summary> Converts a CultureInfo's NativeName String to a CultureInfo. </summary>
         ''' <param name="value">      CultureInfo's NativeName String value. </param>
         ''' <param name="targetType"> System.Type to convert to. </param>
         ''' <param name="parameter">  Ignored. </param>
         ''' <param name="culture">    Ignored. </param>
         ''' <returns>                 The matching CultureInfo. </returns>
        Public Function ConvertBack(value As Object, targetType As Type, parameter As Object, culture As System.Globalization.CultureInfo) As Object Implements IValueConverter.ConvertBack
            Dim ret As Object = Nothing
            
            if (not NativeName2CultureInfo.ContainsKey(value)) then
                Debug.WriteLine(String.Format("CultureInfoConverter[ConvertBack]: can't get CultureInfo for NativeName", value))
            else
                ret = NativeName2CultureInfo(value)
            end if
            
            Return ret
        End Function
    End Class
    
End Namespace

' for jEdit:  :collapseFolds=3::tabSize=4::indentSize=4:
