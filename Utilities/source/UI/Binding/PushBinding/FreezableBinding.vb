
Imports System.Windows
Imports System.Windows.Data
Imports System.ComponentModel

Namespace UI.Binding.PushBinding
    
    ''' <summary>  Part of PushBindingExtension, which allows to bind a read-only dependency property in mode "OneWayToSource" </summary>
     ''' <remarks> Found at http://meleak.wordpress.com/ (many thanks!) and converted to VisualBasic. </remarks>
    Public Class FreezableBinding
        Inherits Freezable
        
        #Region "Properties"
            
            Private _binding As Data.Binding
            
            Protected ReadOnly Property Binding() As Data.Binding
                Get
                    If _binding Is Nothing Then
                        _binding = New Data.Binding()
                    End If
                    Return _binding
                End Get
            End Property
            
            '<DefaultValue(Nothing)> _
            Public Property AsyncState() As Object
                Get
                    Return Binding.AsyncState
                End Get
                Set
                    Binding.AsyncState = value
                End Set
            End Property
            
            <DefaultValue(False)> _
            Public Property BindsDirectlyToSource() As Boolean
                Get
                    Return Binding.BindsDirectlyToSource
                End Get
                Set
                    Binding.BindsDirectlyToSource = value
                End Set
            End Property
            
            '<DefaultValue(Nothing)> _
            Public Property Converter() As IValueConverter
                Get
                    Return Binding.Converter
                End Get
                Set
                    Binding.Converter = value
                End Set
            End Property
            
            '<TypeConverter(GetType(CultureInfoIetfLanguageTagConverter)), DefaultValue(Nothing)> _
            <TypeConverter(GetType(CultureInfoIetfLanguageTagConverter))> _
            Public Property ConverterCulture() As System.Globalization.CultureInfo
                Get
                    Return Binding.ConverterCulture
                End Get
                Set
                    Binding.ConverterCulture = value
                End Set
            End Property
            
            '<DefaultValue(Nothing)> _
            Public Property ConverterParameter() As Object
                Get
                    Return Binding.ConverterParameter
                End Get
                Set
                    Binding.ConverterParameter = value
                End Set
            End Property
            
            '<DefaultValue(Nothing)> _
            Public Property ElementName() As String
                Get
                    Return Binding.ElementName
                End Get
                Set
                    Binding.ElementName = value
                End Set
            End Property
            
            '<DefaultValue(Nothing)> _
            Public Property FallbackValue() As Object
                Get
                    Return Binding.FallbackValue
                End Get
                Set
                    Binding.FallbackValue = value
                End Set
            End Property
            
            <DefaultValue(False)> _
            Public Property IsAsync() As Boolean
                Get
                    Return Binding.IsAsync
                End Get
                Set
                    Binding.IsAsync = value
                End Set
            End Property
            
            <DefaultValue(BindingMode.[Default])> _
            Public Property Mode() As BindingMode
                Get
                    Return Binding.Mode
                End Get
                Set
                    Binding.Mode = value
                End Set
            End Property
            
            <DefaultValue(False)> _
            Public Property NotifyOnSourceUpdated() As Boolean
                Get
                    Return Binding.NotifyOnSourceUpdated
                End Get
                Set
                    Binding.NotifyOnSourceUpdated = value
                End Set
            End Property
            
            <DefaultValue(False)> _
            Public Property NotifyOnTargetUpdated() As Boolean
                Get
                    Return Binding.NotifyOnTargetUpdated
                End Get
                Set
                    Binding.NotifyOnTargetUpdated = value
                End Set
            End Property
            
            <DefaultValue(False)> _
            Public Property NotifyOnValidationError() As Boolean
                Get
                    Return Binding.NotifyOnValidationError
                End Get
                Set
                    Binding.NotifyOnValidationError = value
                End Set
            End Property
            
            '<DefaultValue(Nothing)> _
            Public Property Path() As PropertyPath
                Get
                    Return Binding.Path
                End Get
                Set
                    Binding.Path = value
                End Set
            End Property
            
            '<DefaultValue(Nothing)> _
            Public Property RelativeSource() As RelativeSource
                Get
                    Return Binding.RelativeSource
                End Get
                Set
                    Binding.RelativeSource = value
                End Set
            End Property
            
            '<DefaultValue(Nothing)> _
            Public Property Source() As Object
                Get
                    Return Binding.Source
                End Get
                Set
                    Binding.Source = value
                End Set
            End Property
            
            <DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)> _
            Public Property UpdateSourceExceptionFilter() As UpdateSourceExceptionFilterCallback
                Get
                    Return Binding.UpdateSourceExceptionFilter
                End Get
                Set
                    Binding.UpdateSourceExceptionFilter = value
                End Set
            End Property
            
            <DefaultValue(UpdateSourceTrigger.PropertyChanged)> _
            Public Property UpdateSourceTrigger() As UpdateSourceTrigger
                Get
                    Return Binding.UpdateSourceTrigger
                End Get
                Set
                    Binding.UpdateSourceTrigger = value
                End Set
            End Property
            
            <DefaultValue(False)> _
            Public Property ValidatesOnDataErrors() As Boolean
                Get
                    Return Binding.ValidatesOnDataErrors
                End Get
                Set
                    Binding.ValidatesOnDataErrors = value
                End Set
            End Property
            
            <DefaultValue(False)> _
            Public Property ValidatesOnExceptions() As Boolean
                Get
                    Return Binding.ValidatesOnExceptions
                End Get
                Set
                    Binding.ValidatesOnExceptions = value
                End Set
            End Property
            
            '<DefaultValue(Nothing)> _
            Public Property XPath() As String
                Get
                    Return Binding.XPath
                End Get
                Set
                    Binding.XPath = value
                End Set
            End Property
            
            '<DefaultValue(Nothing)> _
            Public ReadOnly Property ValidationRules() As System.Collections.ObjectModel.Collection(Of System.Windows.Controls.ValidationRule)
                Get
                    Return Binding.ValidationRules
                End Get
            End Property
            
        #End Region
        
        #Region "Freezable overrides"
            
            Protected Overrides Sub CloneCore(sourceFreezable As Freezable)
                
                Dim freezableBindingClone As FreezableBinding = TryCast(sourceFreezable, FreezableBinding)
                
                If (freezableBindingClone.ElementName IsNot Nothing) Then
                    ElementName = freezableBindingClone.ElementName
                ElseIf (freezableBindingClone.RelativeSource IsNot Nothing) Then
                    RelativeSource = freezableBindingClone.RelativeSource
                ElseIf (freezableBindingClone.Source IsNot Nothing) Then
                    Source = freezableBindingClone.Source
                End If
                
                AsyncState = freezableBindingClone.AsyncState
                BindsDirectlyToSource = freezableBindingClone.BindsDirectlyToSource
                Converter = freezableBindingClone.Converter
                ConverterCulture = freezableBindingClone.ConverterCulture
                ConverterParameter = freezableBindingClone.ConverterParameter
                FallbackValue = freezableBindingClone.FallbackValue
                IsAsync = freezableBindingClone.IsAsync
                Mode = freezableBindingClone.Mode
                NotifyOnSourceUpdated = freezableBindingClone.NotifyOnSourceUpdated
                NotifyOnTargetUpdated = freezableBindingClone.NotifyOnTargetUpdated
                NotifyOnValidationError = freezableBindingClone.NotifyOnValidationError
                Path = freezableBindingClone.Path
                UpdateSourceExceptionFilter = freezableBindingClone.UpdateSourceExceptionFilter
                UpdateSourceTrigger = freezableBindingClone.UpdateSourceTrigger
                ValidatesOnDataErrors = freezableBindingClone.ValidatesOnDataErrors
                ValidatesOnExceptions = freezableBindingClone.ValidatesOnExceptions
                XPath = XPath
                
                For Each validationRule As System.Windows.Controls.ValidationRule In freezableBindingClone.ValidationRules
                    ValidationRules.Add(validationRule)
                Next
                
                MyBase.CloneCore(sourceFreezable)
            End Sub
            
            Protected Overrides Function CreateInstanceCore() As Freezable
                Return New FreezableBinding()
            End Function
            
        #End Region
        
    End Class
    
End Namespace

' for jEdit:  :collapseFolds=2::tabSize=4::indentSize=4:
