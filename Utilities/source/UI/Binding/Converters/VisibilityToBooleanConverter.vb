
Imports System
Imports System.Windows.Data
Imports System.Windows


Namespace UI.Binding.Converters
    
    '  Found at http://social.msdn.microsoft.com/Forums/en-US/wpf/thread/f2154f63-ccb5-4d6d-8c01-81f9da9ab347/
    '  <src:VisToBool x:Key="TrueIfVisible" Inverted="False" Not="False" />
    '  <src:VisToBool x:Key="TrueIfNotVisible" Inverted="False" Not="True" />
    '  <src:VisToBool x:Key="VisibleIfTrue" Inverted="True" Not="False" />
    '  <src:VisToBool x:Key="VisibleIfNotTrue" Inverted="True" Not="True" />
    '  
    
    ''' <summary> Converts between Boolean property and Visibility state - and vice versa. </summary>
     ''' <remarks> The converter inverts it's direction when <see cref="VisibilityToBooleanConverter.Inverted"/> property is <see langword="true"/>. </remarks>
    <ValueConversion(GetType(Object), GetType(Object))>
    Public Class VisibilityToBooleanConverter
        Implements IValueConverter
        
        Public Sub New()
        End Sub
        
        
        Private _inverted As Boolean = False
        Private _not As Boolean = False
        
        ''' <summary> If this property is <see langword="true"/>, the converter inverts it's direction. </summary>
        Public Property Inverted() As Boolean
            Get
                Return _inverted
            End Get
            Set
                _inverted = value
            End Set
        End Property
        
        ''' <summary> <see langword="true"/> means "TrueIfNotVisible" (or "VisibleIfNotTrue" if <see cref="Inverted"/> is <see langword="true"/>. </summary>
        Public Property [Not]() As Boolean
            Get
                Return _not
            End Get
            Set
                _not = value
            End Set
        End Property
        
        Private Function VisibilityToBool(value As Object) As Object
            'If Not (TypeOf value Is Visibility) Then
            '    Return DependencyProperty.UnsetValue
            'End If
            
            Return ((DirectCast(value, Visibility) = Visibility.Visible) Xor Me.Not)
        End Function
        
        Private Function BoolToVisibility(value As Object) As Object
            'If Not (TypeOf value Is Boolean) Then
            '    Return DependencyProperty.UnsetValue
            'End If
            
            Return If((CBool(value) Xor Me.Not), Visibility.Visible, Visibility.Collapsed)
        End Function
        
        ''' <summary> Converts from source to target. </summary>
         ''' <param name="value">      Object value. </param>
         ''' <param name="targetType"> System.Type to convert to. </param>
         ''' <param name="parameter">  Ignored. </param>
         ''' <param name="culture">    Ignored. </param>
         ''' <returns>                 Converted value. On error the input value itself is returned. </returns>
        Public Function Convert(value As Object, targetType As Type, parameter As Object, culture As System.Globalization.CultureInfo) As Object Implements IValueConverter.Convert
            Try
                Return If(Me.Inverted, BoolToVisibility(value), VisibilityToBool(value))
                
            Catch ex As System.Exception
                System.Diagnostics.Trace.WriteLine(ex)
                Return value
            End Try
        End Function
        
        ''' <summary> Converts from target to source. </summary>
         ''' <param name="value">      Object value. </param>
         ''' <param name="targetType"> System.Type to convert to. </param>
         ''' <param name="parameter">  Ignored. </param>
         ''' <param name="culture">    Ignored. </param>
         ''' <returns>                 Converted value. On error the input value itself is returned. </returns>
        Public Function ConvertBack(value As Object, targetType As Type, parameter As Object, culture As System.Globalization.CultureInfo) As Object Implements IValueConverter.ConvertBack
            Try
                Return If(Me.Inverted, VisibilityToBool(value), BoolToVisibility(value))
                
            Catch ex As System.Exception
                System.Diagnostics.Trace.WriteLine(ex)
                Return value
            End Try
        End Function
    End Class
    
End Namespace

' for jEdit:  :collapseFolds=3::tabSize=4::indentSize=4:
