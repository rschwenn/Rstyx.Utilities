
'Namespace Rstyx.Utilities
    
    ''' <summary>
    ''' The <b>Rstyx.Utilities</b> Namespace stands for a Class Library
    ''' with general-purpose development utilities.
    ''' </summary>
    <System.Runtime.CompilerServices.CompilerGeneratedAttribute()> _
    Class NamespaceDoc
    End Class
    
    Namespace Apps
        ''' <summary>
        ''' General-purpose development utilities for dealing with applications
        ''' </summary>
        <System.Runtime.CompilerServices.CompilerGeneratedAttribute()> _
        Class NamespaceDoc
        End Class
    End Namespace
    
    Namespace Collections
        ''' <summary>
        ''' General-purpose development utilities for dealing with collections.
        ''' </summary>
        <System.Runtime.CompilerServices.CompilerGeneratedAttribute()> _
        Class NamespaceDoc
        End Class
    End Namespace
    
    Namespace IO
        ''' <summary>
        ''' General-purpose development utilities for input/output.
        ''' </summary>
        <System.Runtime.CompilerServices.CompilerGeneratedAttribute()> _
        Class NamespaceDoc
        End Class
    End Namespace
    
    Namespace IO.CSV
        ''' <summary>
        ''' Handling of CSV files. (CSV Reader: see http://www.codeproject.com/Articles/9258/A-Fast-CSV-Reader)
        ''' </summary>
        <System.Runtime.CompilerServices.CompilerGeneratedAttribute()> _
        Class NamespaceDoc
        End Class
    End Namespace
    
    Namespace UI
        ''' <summary>
        ''' General-purpose development utilities for dealing with WPF driven UI.
        ''' </summary>
        <System.Runtime.CompilerServices.CompilerGeneratedAttribute()> _
        Class NamespaceDoc
        End Class
    End Namespace
    
    Namespace UI.Binding
        ''' <summary>
        ''' WPF binding releted utilities.
        ''' </summary>
        <System.Runtime.CompilerServices.CompilerGeneratedAttribute()> _
        Class NamespaceDoc
        End Class
    End Namespace
    
    Namespace UI.Binding.Converters
        ''' <summary>
        ''' WPF binding converters.
        ''' </summary>
        <System.Runtime.CompilerServices.CompilerGeneratedAttribute()> _
        Class NamespaceDoc
        End Class
    End Namespace
    
    Namespace UI.Binding.PushBinding
        ''' <summary>
        ''' WPF binding support: Binds a read-only dependency property target in mode "One Way to Source".
        ''' </summary>
        ''' <remarks>
        ''' <para>
        ''' Found at http://meleak.wordpress.com/ (many thanks!) and converted to VisualBasic.
        ''' </para>
        ''' <para>
        ''' <b>CAUTION:</b> The binding source property should belong directly to the datacontext,
        ''' otherwise PushBinding may fail. This is expected to be happen especially
        ''' when the binding source object isn't already there when PushBinding is initailized.
        ''' </para>
        ''' </remarks>
        ''' <example>
        ''' Binding the resulting <c>FilePath</c> property of a Rstyx.Utilities.UI.Controls.FileChooser
        ''' to a view model's <c>OutputFilePath</c> property can be done this way:
        ''' <code>
        ''' xmlns:RstyxPush="clr-namespace:Rstyx.Utilities.UI.Binding.PushBinding;assembly=Rstyx.Utilities"
        ''' xmlns:RstyxControls="clr-namespace:Rstyx.Utilities.UI.Controls;assembly=Rstyx.Utilities"
        '''
        ''' &lt;RstyxControls:FileChooser
        '''     FileMode="OpenOrCreate"
        '''     FileFilter="{Binding Path=ActiveExportModule.OutputFileFilter, Mode=OneWay}"
        '''     FileFilterIndex="{Binding Path=ActiveExportModule.OutputFileFilterIndex, Mode=OneWay}"
        '''     InputFilePath="{Binding Path=RawOutputFilePath, Mode=OneWay}"
        '''     &gt;
        '''     &lt;RstyxPush:PushBindingManager.PushBindings&gt;
        '''         &lt;RstyxPush:PushBinding TargetProperty="FilePath" Path="OutputFilePath" /&gt;
        '''     &lt;/RstyxPush:PushBindingManager.PushBindings&gt;
        ''' &lt;/RstyxControls:FileChooser&gt;
        ''' </code>
        ''' </example>
        <System.Runtime.CompilerServices.CompilerGeneratedAttribute()> _
        Class NamespaceDoc
        End Class
    End Namespace
    
    Namespace UI.Controls
        ''' <summary>
        ''' General-purpose WPF user controls.
        ''' </summary>
        <System.Runtime.CompilerServices.CompilerGeneratedAttribute()> _
        Class NamespaceDoc
        End Class
    End Namespace
    
    Namespace UI.Resources
        ''' <summary>
        ''' General-purpose WFP resources.
        ''' </summary>
        <System.Runtime.CompilerServices.CompilerGeneratedAttribute()> _
        Class NamespaceDoc
        End Class
    End Namespace
    
    Namespace UI.ViewModel
        ''' <summary>
        ''' View model related stuff (MVVM).
        ''' </summary>
        <System.Runtime.CompilerServices.CompilerGeneratedAttribute()> _
        Class NamespaceDoc
        End Class
    End Namespace
    
'End Namespace

' for jEdit:  :collapseFolds=2::tabSize=4::indentSize=4:
