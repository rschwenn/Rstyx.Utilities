Imports MS.Internal.KnownBoxes
Imports System
Imports System.ComponentModel
Imports System.Windows.Automation.Peers
Imports System.Windows.Input
Imports System.Windows.Markup

Namespace System.Windows.Controls
	<Localizability(LocalizationCategory.Label)>
	Public Class Label
		Inherits ContentControl

		Public Shared TargetProperty As DependencyProperty

		Private Shared LabeledByProperty As DependencyProperty

		Private Shared _dType As DependencyObjectType

		<TypeConverter(GetType(NameReferenceConverter))>
		Public Property Target() As UIElement
			Get
				Return CType(MyBase.GetValue(Label.TargetProperty), UIElement)
			End Get
			Set(value As UIElement)
				MyBase.SetValue(Label.TargetProperty, value)
			End Set
		End Property

		Friend Override ReadOnly Property DTypeThemeStyleKey() As DependencyObjectType
			Get
				Return Label._dType
			End Get
		End Property

		Shared Sub New()
			Label.TargetProperty = DependencyProperty.Register("Target", GetType(UIElement), GetType(Label), New FrameworkPropertyMetadata(Nothing, New PropertyChangedCallback(Label.OnTargetChanged)))
			Label.LabeledByProperty = DependencyProperty.RegisterAttached("LabeledBy", GetType(Label), GetType(Label), New FrameworkPropertyMetadata(Nothing))
			EventManager.RegisterClassHandler(GetType(Label), AccessKeyManager.AccessKeyPressedEvent, New AccessKeyPressedEventHandler(Label.OnAccessKeyPressed))
			FrameworkElement.DefaultStyleKeyProperty.OverrideMetadata(GetType(Label), New FrameworkPropertyMetadata(GetType(Label)))
			Label._dType = DependencyObjectType.FromSystemTypeInternal(GetType(Label))
			Control.IsTabStopProperty.OverrideMetadata(GetType(Label), New FrameworkPropertyMetadata(BooleanBoxes.FalseBox))
			UIElement.FocusableProperty.OverrideMetadata(GetType(Label), New FrameworkPropertyMetadata(BooleanBoxes.FalseBox))
		End Sub

		Private Shared Sub OnTargetChanged(d As DependencyObject, e As DependencyPropertyChangedEventArgs)
			Dim label As Label = CType(d, Label)
			Dim uIElement As UIElement = CType(e.OldValue, UIElement)
			Dim uIElement2 As UIElement = CType(e.NewValue, UIElement)
			If uIElement IsNot Nothing Then
				Dim value As Object = uIElement.GetValue(Label.LabeledByProperty)
				If value Is label Then
					uIElement.ClearValue(Label.LabeledByProperty)
				End If
			End If
			If uIElement2 IsNot Nothing Then
				uIElement2.SetValue(Label.LabeledByProperty, label)
			End If
		End Sub

		Friend Shared Function GetLabeledBy(o As DependencyObject) As Label
			If o Is Nothing Then
				Throw New ArgumentNullException("o")
			End If
			Return CType(o.GetValue(Label.LabeledByProperty), Label)
		End Function

		Protected Override Function OnCreateAutomationPeer() As AutomationPeer
			Return New LabelAutomationPeer(Me)
		End Function

		Private Shared Sub OnAccessKeyPressed(sender As Object, e As AccessKeyPressedEventArgs)
			Dim label As Label = TryCast(sender, Label)
			If Not e.Handled AndAlso e.Scope Is Nothing AndAlso (e.Target Is Nothing OrElse e.Target Is label) Then
				e.Target = label.Target
			End If
		End Sub
	End Class
End Namespace
