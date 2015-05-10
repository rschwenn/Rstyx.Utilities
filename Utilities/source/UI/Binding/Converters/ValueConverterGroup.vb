
Imports System
Imports System.Collections
Imports System.Collections.Generic
Imports System.Collections.ObjectModel
Imports System.Collections.Specialized
Imports System.Globalization
Imports System.Windows.Data

Namespace UI.Binding.Converters
    ''' <summary>
    ''' <para>
    ''' Thanks to Josh: http://www.codeproject.com/Articles/15061/Piping-Value-Converters-in-WPF
    ''' </para>
    ''' <para>
    ''' A value converter which contains a list of IValueConverters and invokes their Convert or ConvertBack methods
    ''' in the order that they exist in the list.  The output of one converter is piped into the next converter
    ''' allowing for modular value converters to be chained together.  If the ConvertBack method is invoked, the
    ''' value converters are executed in reverse order (highest to lowest index).  Do not leave an element in the
    ''' Converters property collection null, every element must reference a valid IValueConverter instance. If a
    ''' value converter's type is not decorated with the ValueConversionAttribute, an InvalidOperationException will be
    ''' thrown when the converter is added to the Converters collection.
    ''' </para>
    ''' </summary>
    <System.Windows.Markup.ContentProperty("Converters")> _
    Public Class ValueConverterGroup
        Implements IValueConverter
        
        #Region "Private Fields"
            
            Private ReadOnly m_converters     As ObservableCollection(Of IValueConverter) = New ObservableCollection(Of IValueConverter)()
            Private ReadOnly cachedAttributes As Dictionary(Of IValueConverter, ValueConversionAttribute) = New Dictionary(Of IValueConverter, ValueConversionAttribute)()
            
        #End Region
        
        #Region "Constructor"
            
            Public Sub New()
                AddHandler Me.m_converters.CollectionChanged, AddressOf Me.OnConvertersCollectionChanged
            End Sub
            
        #End Region
        
        #Region "Converters"
            
            ''' <summary> Returns the list of IValueConverters contained in this converter.  </summary>
            Public ReadOnly Property Converters() As ObservableCollection(Of IValueConverter)
                Get
                    Return Me.m_converters
                End Get
            End Property
            
        #End Region
        
        #Region "IValueConverter Members"
            
            Private Function IValueConverter_Convert(value As Object, targetType As Type, parameter As Object, culture As CultureInfo) As Object Implements IValueConverter.Convert
                Dim output As Object = value
                
                For i As Integer = 0 To Me.Converters.Count - 1
                    Dim converter As IValueConverter = Me.Converters(i)
                    Dim currentTargetType As Type = Me.GetTargetType(i, targetType, True)
                    output = converter.Convert(output, currentTargetType, parameter, culture)
                    
                    ' If the converter returns 'DoNothing' then the binding operation should terminate.
                    If (output Is System.Windows.Data.Binding.DoNothing) Then
                        Exit For
                    End If
                Next
                
                Return output
            End Function
            
            Private Function IValueConverter_ConvertBack(value As Object, targetType As Type, parameter As Object, culture As CultureInfo) As Object Implements IValueConverter.ConvertBack
                Dim output As Object = value
                
                For i As Integer = Me.Converters.Count - 1 To -1 + 1 Step -1
                    Dim converter As IValueConverter = Me.Converters(i)
                    Dim currentTargetType As Type = Me.GetTargetType(i, targetType, False)
                    output = converter.ConvertBack(output, currentTargetType, parameter, culture)
                    
                    ' When a converter returns 'DoNothing' the binding operation should terminate.
                    If (output Is System.Windows.Data.Binding.DoNothing) Then
                        Exit For
                    End If
                Next
                
                Return output
            End Function
            
        #End Region
        
        #Region "Private Helpers"
            
            ''' <summary> Returns the target type for a conversion operation. </summary>
            ''' <param name="converterIndex">The index of the current converter about to be executed.</param>
            ''' <param name="finalTargetType">The 'targetType' argument passed into the conversion method.</param>
            ''' <param name="convert">Pass true if calling from the Convert method, or false if calling from ConvertBack.</param>
            Protected Overridable Function GetTargetType(converterIndex As Integer, finalTargetType As Type, convert As Boolean) As Type
                ' If the current converter is not the last/first in the list, 
                ' get a reference to the next/previous converter.
                Dim nextConverter As IValueConverter = Nothing
                If (convert) Then
                    If (converterIndex < Me.Converters.Count - 1) Then
                        nextConverter = Me.Converters(converterIndex + 1)
                        If (nextConverter Is Nothing) Then
                            Throw New InvalidOperationException("The Converters collection of the ValueConverterGroup contains a null reference at index: " & (converterIndex + 1))
                        End If
                    End If
                Else
                    If (converterIndex > 0) Then
                        nextConverter = Me.Converters(converterIndex - 1)
                        If (nextConverter Is Nothing) Then
                            Throw New InvalidOperationException("The Converters collection of the ValueConverterGroup contains a null reference at index: " & (converterIndex - 1))
                        End If
                    End If
                End If
                
                If (nextConverter IsNot Nothing) Then
                    Dim conversionAttribute As ValueConversionAttribute = cachedAttributes(nextConverter)
                    
                    ' If the Convert method is going to be called, we need to use the SourceType of the next 
                    ' converter in the list.  If ConvertBack is called, use the TargetType.
                    Return If(convert, conversionAttribute.SourceType, conversionAttribute.TargetType)
                End If
                
                ' If the current converter is the last one to be executed return the target type passed into the conversion method.
                Return finalTargetType
            End Function
            
            Private Sub OnConvertersCollectionChanged(sender As Object, e As NotifyCollectionChangedEventArgs)
                ' The 'Converters' collection has been modified, so validate that each value converter it now
                ' contains is decorated with ValueConversionAttribute and then cache the attribute value.
                
                Dim convertersToProcess As IList = Nothing
                If (e.Action = NotifyCollectionChangedAction.Add OrElse e.Action = NotifyCollectionChangedAction.Replace) Then
                    convertersToProcess = e.NewItems
                ElseIf (e.Action = NotifyCollectionChangedAction.Remove) Then
                    For Each converter As IValueConverter In e.OldItems
                        Me.cachedAttributes.Remove(converter)
                    Next
                ElseIf (e.Action = NotifyCollectionChangedAction.Reset) Then
                    Me.cachedAttributes.Clear()
                    convertersToProcess = Me.m_converters
                End If
                
                If (convertersToProcess IsNot Nothing AndAlso convertersToProcess.Count > 0) Then
                    For Each converter As IValueConverter In convertersToProcess
                        Dim attributes As Object() = converter.[GetType]().GetCustomAttributes(GetType(ValueConversionAttribute), False)
                        
                        If (attributes.Length <> 1) Then
                            Throw New InvalidOperationException("All value converters added to a ValueConverterGroup must be decorated with the ValueConversionAttribute attribute exactly once.")
                        End If
                        
                        Me.cachedAttributes.Add(converter, TryCast(attributes(0), ValueConversionAttribute))
                    Next
                End If
            End Sub
            
        #End Region
        
    End Class
    
End Namespace

' for jEdit:  :collapseFolds=3::tabSize=4::indentSize=4:
