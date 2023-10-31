
Imports System
Imports System.Collections.Generic
Imports System.Reflection
Imports System.Text.RegularExpressions

Imports Rstyx.Utilities.IO
Imports Rstyx.Utilities.Math.MathExtensions
Imports Rstyx.Utilities.StringUtils

Namespace Domain
    
    ''' <summary> Constraints to point values that are required for a certain purpose. </summary>
    <Flags>
    Public Enum GeoPointConstraints As Integer
        
        ''' <summary> No constraints. </summary>
        None = 0
        
        ''' <summary> The point's position has to be known, hence X and Y must not be <c>Double.NaN</c>. </summary>
        KnownPosition = 1
        
        ''' <summary> The point's height has to be known, hence Z must not be <c>Double.NaN</c>. </summary>
        KnownHeight = 2
                
        ''' <summary> Values in canted rails system are required. </summary>
         ''' <remarks> This requires a point that implements the <see cref="IPointAtTrackGeometry"/> interface. </remarks>
        KnownCantedRailsSystem = 4
        
        ''' <summary> The point's ID has to be unique in a list of points. </summary>
        UniqueID = 8
        
        ''' <summary> The point's ID has to be unique inside a block of points (see <see cref="Rstyx.Utilities.Domain.IO.TcFileReader.Blocks"/>). </summary>
        UniqueIDPerBlock = 16
        
    End Enum
    
    ''' <summary> Point edit options (i.e. for applying while reading from file). </summary>
    <Flags>
    Public Enum GeoPointEditOptions As Integer
        
        ''' <summary> No editing is applied. </summary>
        None = 0
        
        ''' <summary> The point's <see cref="GeoPoint.Info"/> should be parsed for <see cref="GeoPoint.ActualCant"/> and <see cref="GeoPoint.ActualCantAbs"/>. </summary>
         ''' <remarks>
         ''' If <see cref="GeoPoint.Kind"/> is <c>None</c> or <c>Rails</c>, then <see cref="GeoPoint.ParseInfoForActualCant"/> 
         ''' should be invoked to extract a cant value. 
         ''' </remarks>
        ParseInfoForActualCant = 1
        
        ''' <summary> The point's <see cref="GeoPoint.Info"/> should be parsed for <see cref="GeoPoint.Kind"/>. </summary>
         ''' <remarks>
         ''' If <see cref="GeoPoint.Kind"/> is <c>None</c>, then <see cref="GeoPoint.ParseInfoForPointKind"/> 
         ''' should be invoked to guess point kind.
         ''' </remarks>
        ParseInfoForPointKind = 2
        
        ''' <summary> The ipkt "Text" field should be parsed as iGeo "iTrassen-Codierung". </summary>
         ''' <remarks>
         ''' When reading a <see cref="GeoIPoint"/> from <see cref="IO.iPktFile"/>, then <see cref="GeoIPoint.Parse_iTC"/> 
         ''' should be invoked to extract some values. 
         ''' </remarks>
        Parse_iTC = 4
        
        ''' <summary> The <see cref="GeoPoint.Comment"/> should be parsed too, if <see cref="GeoPoint.Kind"/> is <c>None</c> after parsing <see cref="GeoPoint.Info"/>.</summary>
        ParseCommentToo = 8
        
    End Enum
    
    ''' <summary> Point output options (i.e. for applying while writing to file). </summary>
    <Flags>
    Public Enum GeoPointOutputOptions As Integer
        
        ''' <summary> No special output is applied. </summary>
        None = 0
        
        ''' <summary> The point's <see cref="GeoPoint.ActualCant"/> and <see cref="GeoPoint.ActualCantAbs"/> should be added to <see cref="GeoPoint.Info"/> for output. </summary>
        CreateInfoWithActualCant = 1
        
        ''' <summary> The point's <see cref="GeoPoint.Kind"/> should be added to <see cref="GeoPoint.Info"/> for output. </summary>
         ''' <remarks>
         ''' If <see cref="GeoPoint.Kind"/> isn't <c>None</c>, a kind text should be added to <see cref="GeoPoint.Info"/>
         ''' unless there is already a point kind descriptor in there. 
         ''' This statement includes also <see cref="GeoPointOutputOptions.CreateInfoWithActualCant"/>
         ''' </remarks>
        CreateInfoWithPointKind = 2
        
        ''' <summary> Create iGeo "iTrassen-Codierung" for output of ipkt Text field. </summary>
        Create_iTC = 4
        
    End Enum
    
    ''' <summary> Supported point kinds. </summary>
    Public Enum GeoPointKind As Integer
        
        ''' <summary> Not a supported point kind. </summary>
        None = 0
        
        ''' <summary> Rails axis. </summary>
        Rails = 1
        
        ''' <summary> Platform edge. </summary>
        Platform = 2
                
        ''' <summary> General fix point. </summary>
        FixPoint = 3
                
        ''' <summary> Height fix point. </summary>
        FixPoint1D = 4
                
        ''' <summary> Position fix point. </summary>
        FixPoint2D = 5
                
        ''' <summary> Position and Height fix point. </summary>
        FixPoint3D = 6
        
        ''' <summary> Rails fix point. </summary>
        RailsFixPoint = 7
        
        ''' <summary> Measure point. </summary>
        MeasurePoint = 8
        
        ''' <summary> Measure point 1. </summary>
        MeasurePoint1 = 9
        
        ''' <summary> Measure point 2. </summary>
        MeasurePoint2 = 10
        
        ''' <summary> Top of rail. </summary>
        RailTop = 11
        
        ''' <summary> Top of rail 1. </summary>
        RailTop1 = 12
        
        ''' <summary> Top of rail 2. </summary>
        RailTop2 = 13
        
    End Enum
    
    ''' <summary> Hints about point status. </summary>
    <Flags>
    Public Enum GeoPointStatusHints As Integer
        
        ''' <summary> No hints. </summary>
        None = 0
        
        ''' <summary> The point ID may be bad i.e. in means of a schema. </summary>
        BadID = 1
        
        ''' <summary> The geometry values are not exactly for the point, but rather sector extreme values. </summary>
        ExtremeGeometry = 2
        
    End Enum
    
    
    ''' <summary> Representation of a geodetic point including some point info. </summary>
     ''' <remarks></remarks>
    Public Class GeoPoint
        Implements IGeoPoint
        
        #Region "Protected Fields"
            
            Protected Shared Logger As Rstyx.LoggingConsole.Logger = Rstyx.LoggingConsole.LogBox.GetLogger("Rstyx.Utilities.Domain.GeoPoint")
            
            ''' <summary> Patterns for recognizing actual cant from info text. </summary>
            Protected Shared ReadOnly InfoCantPatterns  As Dictionary(Of String, String)
            
            ''' <summary> Mapping:  Patterns for recognizing point kind from info text => point kind. </summary>
            Protected Shared ReadOnly InfoKindPatterns  As Dictionary(Of String, GeoPointKind)
            
            ''' <summary> Mapping:  Property name => Attribute name. </summary>
            Protected Shared ReadOnly MarkType2Kind     As Dictionary(Of String, GeoPointKind)
            
            ''' <summary> Mapping:  KindText => Kind. </summary>
            Protected Shared ReadOnly KindText2Kind     As Dictionary(Of String, GeoPointKind)
            
            
            ''' <summary> Mapping:  Kind => Default KindText. </summary>
             ''' <remarks> A derived class may override default mappings. </remarks>
            Protected ReadOnly DefaultKindText As Dictionary(Of GeoPointKind, String)
            
            ''' <summary> Point type dependent Mapping:  Property name => Attribute name </summary>
             ''' <remarks>
             ''' <para>
             ''' This defaults to a list of selected attributes matching <see cref="IGeoPoint"/> interface,
             ''' hence <see cref="GeoPoint"/> properties. A derived class may add mappings.
             ''' </para>
             ''' <para>
             ''' <see cref="GetPropsFromIGeoPoint"/> should create these attributes from those properties,
             ''' which do not belong to <see cref="IGeoPoint"/> interface.
             ''' </para>
             ''' <para>
             ''' <see cref="RemovePropertyAttributes"/> removes all these attributes from <see cref="Attributes"/>.
             ''' </para>
             ''' <para>
             ''' The constructur of a derived point which takes an <see cref="IGeoPoint"/> to init values,
             ''' should try to restore these attributes to properties.
             ''' </para>
             ''' </remarks>
            Protected ReadOnly PropertyAttributes As Dictionary(Of String, String)
            
        #End Region
        
        #Region "Constuctor"
            
            ''' <summary> Static initializations. </summary>
            Shared Sub New()
                ' Patterns for recognizing actual cant from info text.
                ' 26.03.2021: "=" now is mandatory.
                ' 07.04.2023: Cant value now may be a decimal instead of only integer.
                Dim RegExDecimal As String = " *= *([+-]?([0-9]*[.])?[0-9]+)\s*"
                InfoCantPatterns = New Dictionary(Of String, String)
                InfoCantPatterns.Add("(u)"   & RegExDecimal, "relative or indefinite cant")
                InfoCantPatterns.Add("(ueb)" & RegExDecimal, "absolute cant")
                'InfoCantPatterns.Add("(u) *= *([+-]? *[0-9]+)\s*"  , "relative or indefinite cant")
                'InfoCantPatterns.Add("(ueb) *= *([+-]? *[0-9]+)\s*", "absolute cant")
                
                ' Patterns for recognizing point kind from info text.
                InfoKindPatterns  = New Dictionary(Of String, GeoPointKind)
                InfoKindPatterns.Add("\bGls\b|\bGleis\b"                , GeoPointKind.Rails)
                InfoKindPatterns.Add("\bBst\b|\bBstg\b|\bBahnst"        , GeoPointKind.Platform)
                InfoKindPatterns.Add("\bBst_|\bBstg_"                   , GeoPointKind.Platform)
                InfoKindPatterns.Add("\bPS4\b|\bGVP"                    , GeoPointKind.RailsFixPoint)
                InfoKindPatterns.Add("\bPS3\b|\bHFP\b|\bHB|\bHP"        , GeoPointKind.FixPoint1D)
                InfoKindPatterns.Add("\bPS2\b|\bLFP\b|\bPPB\b|\bP[0-9]+", GeoPointKind.FixPoint2D)
                InfoKindPatterns.Add("\bPS1\b|\bGPSC\b|\bLHFP\b"        , GeoPointKind.FixPoint3D)
                InfoKindPatterns.Add("\bPS0\b|\bNXO\b|\bDBRF\b"         , GeoPointKind.FixPoint3D)
                InfoKindPatterns.Add("\bPP\b|\bAP\b|\bPSx\b"            , GeoPointKind.FixPoint)
                InfoKindPatterns.Add("\bSOK1\b"                         , GeoPointKind.RailTop1)
                InfoKindPatterns.Add("\bSOK2\b"                         , GeoPointKind.RailTop2)
                InfoKindPatterns.Add("\bSOK\b"                          , GeoPointKind.RailTop)
                InfoKindPatterns.Add("\bMesspkt1\b|\bMesspunkt1\b"      , GeoPointKind.MeasurePoint1)
                InfoKindPatterns.Add("\bMesspkt2\b|\bMesspunkt2\b"      , GeoPointKind.MeasurePoint2)
                InfoKindPatterns.Add("\bMesspkt\b|\bMesspunkt\b"        , GeoPointKind.MeasurePoint)
                
                ' Mapping:  KindText => Kind.
                KindText2Kind = New Dictionary(Of String, GeoPointKind)
                KindText2Kind.Add("Gls" , GeoPointKind.Rails)
                KindText2Kind.Add("Bstg", GeoPointKind.Platform)
                KindText2Kind.Add("DBRF", GeoPointKind.FixPoint3D)
                KindText2Kind.Add("GPSC", GeoPointKind.FixPoint3D)
                KindText2Kind.Add("GVPV", GeoPointKind.RailsFixPoint)
                KindText2Kind.Add("GVP" , GeoPointKind.RailsFixPoint)
                KindText2Kind.Add("HBH" , GeoPointKind.FixPoint1D)
                KindText2Kind.Add("HFP" , GeoPointKind.FixPoint1D)
                KindText2Kind.Add("LFP" , GeoPointKind.FixPoint2D)
                KindText2Kind.Add("NXO" , GeoPointKind.FixPoint3D)
                KindText2Kind.Add("PPB" , GeoPointKind.FixPoint2D)
                KindText2Kind.Add("PS0" , GeoPointKind.FixPoint3D)
                KindText2Kind.Add("PS1" , GeoPointKind.FixPoint3D)
                KindText2Kind.Add("PS2" , GeoPointKind.FixPoint2D) 
                KindText2Kind.Add("PS3" , GeoPointKind.FixPoint1D)
                KindText2Kind.Add("PS4" , GeoPointKind.RailsFixPoint)
                KindText2Kind.Add("PSx" , GeoPointKind.FixPoint)
                KindText2Kind.Add("PP"  , GeoPointKind.FixPoint)
                KindText2Kind.Add("AP"  , GeoPointKind.FixPoint)
                KindText2Kind.Add("SOK" , GeoPointKind.RailTop)
                KindText2Kind.Add("SOK1", GeoPointKind.RailTop1)
                KindText2Kind.Add("SOK2", GeoPointKind.RailTop2)
                KindText2Kind.Add("MP"  , GeoPointKind.MeasurePoint)
                KindText2Kind.Add("MP1" , GeoPointKind.MeasurePoint1)
                KindText2Kind.Add("MP2" , GeoPointKind.MeasurePoint2)
                
                ' Mapping:  MarkType => Kind.
                MarkType2Kind = New Dictionary(Of String, GeoPointKind)
                MarkType2Kind.Add("0" , GeoPointKind.None)            ' unvermarkt aus Ril 808 
                MarkType2Kind.Add("1" , GeoPointKind.RailsFixPoint)   ' Bolzen am Fahrleitungsmast / Fundament 
                MarkType2Kind.Add("2" , GeoPointKind.RailsFixPoint)   ' Stehbolzen, Nagel, Bolzen in Mauer 
                MarkType2Kind.Add("3" , GeoPointKind.RailsFixPoint)   ' Bodenvermarkung, Tiefpunkt 
                MarkType2Kind.Add("4" , GeoPointKind.RailsFixPoint)   ' Tunnelvermarkung 
                MarkType2Kind.Add("5" , GeoPointKind.FixPoint2D)      ' Lochstein Grundlagen-Vermessung 
                MarkType2Kind.Add("6" , GeoPointKind.FixPoint2D)      ' Dränrohr 
                MarkType2Kind.Add("7" , GeoPointKind.FixPoint2D)      ' Eisenrohr 
                MarkType2Kind.Add("8" , GeoPointKind.None)            ' indirekte Vermarkung 
                MarkType2Kind.Add("9" , GeoPointKind.FixPoint1D)      ' Höhen- bzw. NivP-Bolzen 
                MarkType2Kind.Add("10", GeoPointKind.FixPoint2D)      ' TP - Stein 
                MarkType2Kind.Add("11", GeoPointKind.FixPoint2D)      ' TP - Platte 
                MarkType2Kind.Add("12", GeoPointKind.None)            ' Grenzstein Kataster-Vermessung 
                MarkType2Kind.Add("13", GeoPointKind.None)            ' Grenzmarke 
                MarkType2Kind.Add("14", GeoPointKind.FixPoint2D)      ' Nagel 
                MarkType2Kind.Add("15", GeoPointKind.FixPoint2D)      ' Kreuz 
                MarkType2Kind.Add("16", GeoPointKind.FixPoint3D)      ' Bolzen mit eingravierter ID DB-Referenzsystem
            End Sub
            
            ''' <summary> Creates a new GeoPoint. </summary>
            Public Sub New()
                ' Mapping:  Property name => Attribute name.
                PropertyAttributes = New Dictionary(Of String, String)
                PropertyAttributes.Add("ActualTrackGauge", Rstyx.Utilities.Resources.Messages.Domain_AttName_ActualTrackGauge)  ' "Spurweite"
                PropertyAttributes.Add("CoordSys"        , Rstyx.Utilities.Resources.Messages.Domain_AttName_CoordSys)          ' "SysL"
                PropertyAttributes.Add("HeightInfo"      , Rstyx.Utilities.Resources.Messages.Domain_AttName_HeightInfo)        ' "TextH"
                PropertyAttributes.Add("HeightSys"       , Rstyx.Utilities.Resources.Messages.Domain_AttName_HeightSys)         ' "SysH"
                PropertyAttributes.Add("Job"             , Rstyx.Utilities.Resources.Messages.Domain_AttName_Job)               ' "Auftrag"
                PropertyAttributes.Add("KindText"        , Rstyx.Utilities.Resources.Messages.Domain_AttName_KindText)          ' "PArt"
                PropertyAttributes.Add("MarkHints"       , Rstyx.Utilities.Resources.Messages.Domain_AttName_MarkHints)         ' "Stabil"
                PropertyAttributes.Add("mh"              , Rstyx.Utilities.Resources.Messages.Domain_AttName_mh)                ' "mh"
                PropertyAttributes.Add("mp"              , Rstyx.Utilities.Resources.Messages.Domain_AttName_mp)                ' "mp"
                PropertyAttributes.Add("sh"              , Rstyx.Utilities.Resources.Messages.Domain_AttName_sh)                ' "SH"
                PropertyAttributes.Add("sp"              , Rstyx.Utilities.Resources.Messages.Domain_AttName_sp)                ' "SL"
                PropertyAttributes.Add("TimeStamp"       , Rstyx.Utilities.Resources.Messages.Domain_AttName_TimeStamp)         ' "Zeit"
                PropertyAttributes.Add("wh"              , Rstyx.Utilities.Resources.Messages.Domain_AttName_wh)                ' "GH"
                PropertyAttributes.Add("wp"              , Rstyx.Utilities.Resources.Messages.Domain_AttName_Wp)                ' "GL"

                ' Mapping:  Kind => Default KindText.
                DefaultKindText = New Dictionary(Of GeoPointKind, String)
                DefaultKindText.Add(GeoPointKind.None          , ""    )
                DefaultKindText.Add(GeoPointKind.FixPoint      , "FP"  )
                DefaultKindText.Add(GeoPointKind.FixPoint1D    , "HFP" )
                DefaultKindText.Add(GeoPointKind.FixPoint2D    , "LFP" )
                DefaultKindText.Add(GeoPointKind.FixPoint3D    , "LHFP")
                DefaultKindText.Add(GeoPointKind.Platform      , "Bstg")
                DefaultKindText.Add(GeoPointKind.Rails         , "Gls" )
                DefaultKindText.Add(GeoPointKind.RailsFixPoint , "GVP" )
                DefaultKindText.Add(GeoPointKind.RailTop       , "SOK" )
                DefaultKindText.Add(GeoPointKind.RailTop1      , "SOK1")
                DefaultKindText.Add(GeoPointKind.RailTop2      , "SOK2")
                DefaultKindText.Add(GeoPointKind.MeasurePoint  , "MP"  )
                DefaultKindText.Add(GeoPointKind.MeasurePoint1 , "MP1" )
                DefaultKindText.Add(GeoPointKind.MeasurePoint2 , "MP2" )
            End Sub
            
            ''' <summary> Creates a new GeoPoint and inititializes it's properties from any given <see cref="IGeoPoint"/>. </summary>
             ''' <param name="SourcePoint"> The source point to get init values from. May be <see langword="null"/>. </param>
             ''' <remarks></remarks>
             ''' <exception cref="InvalidIDException"> ID of <paramref name="SourcePoint"/> isn't a valid ID for this point. </exception>
            Public Sub New(SourcePoint As IGeoPoint)
                Me.New()
                Me.GetPropsFromIGeoPoint(SourcePoint)
            End Sub
            
        #End Region
        
        #Region "IGeoPointConversions Members"
            
            ''' <summary> Returns a <see cref="GeoVEPoint"/> initialized with values of the implementing point. </summary>
             ''' <returns> A <see cref="GeoVEPoint"/> initialized with values of the implementing point. </returns>
             ''' <remarks>
             ''' If the implementing point is already a <see cref="GeoVEPoint"/>, then the same instance will be returned.
             ''' Otherwise a new instance of <see cref="GeoVEPoint"/> will be created.
             ''' </remarks>
            Public Function AsGeoVEPoint() As GeoVEPoint Implements IGeoPointConversions.AsGeoVEPoint
                If (TypeOf Me Is GeoVEPoint) Then
                    Return Me
                Else
                    Return New GeoVEPoint(Me)
                End If
            End Function
            
            ''' <summary> Returns a <see cref="GeoIPoint"/> initialized with values of the implementing point. </summary>
             ''' <returns> A <see cref="GeoIPoint"/> initialized with values of the implementing point. </returns>
             ''' <remarks>
             ''' If the implementing point is already a <see cref="GeoIPoint"/>, then the same instance will be returned.
             ''' Otherwise a new instance of <see cref="GeoIPoint"/> will be created.
             ''' </remarks>
            Public Function AsGeoIPoint() As GeoIPoint Implements IGeoPointConversions.AsGeoIPoint
                If (TypeOf Me Is GeoIPoint) Then
                    Return Me
                Else
                    Return New GeoIPoint(Me)
                End If
            End Function
            
            ''' <summary> Returns a <see cref="GeoTcPoint"/> initialized with values of the implementing point. </summary>
             ''' <returns> A <see cref="GeoTcPoint"/> initialized with values of the implementing point. </returns>
             ''' <remarks>
             ''' If the implementing point is already a <see cref="GeoTcPoint"/>, then the same instance will be returned.
             ''' Otherwise a new instance of <see cref="GeoTcPoint"/> will be created.
             ''' </remarks>
            Public Function AsGeoTcPoint() As GeoTcPoint Implements IGeoPointConversions.AsGeoTcPoint
                If (TypeOf Me Is GeoTcPoint) Then
                    Return Me
                Else
                    Return New GeoTcPoint(Me)
                End If
            End Function
            
            ''' <summary> Sets this point's <see cref="IGeoPoint"/> interface properties from a given <see cref="IGeoPoint"/>. </summary>
             ''' <param name="SourcePoint"> The source point to get init values from. May be <see langword="null"/>. </param>
             ''' <remarks>
             ''' <para> 
             ''' A derived class may override this in order to get values from properties, 
             ''' that don't belong to <see cref="IGeoPoint"/> interface.
             ''' </para>
             ''' Properties from <paramref name="SourcePoint"/>, that are declared in <see cref="PropertyAttributes"/>, 
             ''' but don't belong to <see cref="IGeoPoint"/> interface will be <b>added to attributes</b>:
             ''' <para>
             ''' If <paramref name="SourcePoint"/> is a <see cref="GeoIPoint"/>, then the following properties will be converted to attributes:
             ''' <list type="table">
             ''' <listheader> <term> <b>Property Name</b> </term>  <description> <b>Attribute Name</b> </description></listheader>
             ''' <item> <term> MarkTypeAB </term>  <description> VArtAB </description></item>
             ''' </list>
             ''' </para>
             ''' <para>
             ''' If <paramref name="SourcePoint"/> is a <see cref="GeoVEPoint"/>, then the following properties will be converted to attributes:
             ''' <list type="table">
             ''' <listheader> <term> <b>Property Name</b> </term>  <description> <b>Attribute Name</b> </description></listheader>
             ''' <item> <term> TrackPos.TrackNo   </term>  <description> StrNr </description></item>
             ''' <item> <term> TrackPos.RailsCode </term>  <description> StrRi </description></item>
             ''' <item> <term> TrackPos.Kilometer </term>  <description> StrKm </description></item>
             ''' </list>
             ''' </para>
             ''' <para>
             ''' If <paramref name="SourcePoint"/> is a <see cref="GeoTCPoint"/>, then the following properties will be converted to attributes:
             ''' <list type="table">
             ''' <listheader> <term> <b>Property Name</b> </term>  <description> <b>Attribute Name</b> </description></listheader>
             ''' <item> <term> Km </term>  <description> StrKm </description></item>
             ''' </list>
             ''' </para>
             ''' </remarks>
             ''' <exception cref="InvalidIDException"> ID of <paramref name="SourcePoint"/> isn't a valid ID for this point. </exception>
            Protected Overridable Sub GetPropsFromIGeoPoint(SourcePoint As IGeoPoint)
                
                If (SourcePoint IsNot Nothing) Then
                    
                    Me.ID               = SourcePoint.ID
                    
                    Me.Attributes       = SourcePoint.Attributes?.Clone()
                    Me.ActualCant       = SourcePoint.ActualCant
                    Me.ActualCantAbs    = SourcePoint.ActualCantAbs
                    Me.ActualTrackGauge = SourcePoint.ActualTrackGauge
                    Me.Info             = SourcePoint.Info
                    Me.HeightInfo       = SourcePoint.HeightInfo
                    Me.Comment          = SourcePoint.Comment
                    Me.Kind             = SourcePoint.Kind
                    Me.KindText         = SourcePoint.KindText
                    Me.MarkType         = SourcePoint.MarkType
                    Me.MarkHints        = SourcePoint.MarkHints
                    Me.ObjectKey        = SourcePoint.ObjectKey
                    Me.Job              = SourcePoint.Job
                    Me.TimeStamp        = SourcePoint.TimeStamp
                    
                    Me.X                = SourcePoint.X
                    Me.Y                = SourcePoint.Y
                    Me.Z                = SourcePoint.Z
                    Me.mp               = SourcePoint.mp
                    Me.mh               = SourcePoint.mh
                    Me.wp               = SourcePoint.wp
                    Me.wh               = SourcePoint.wh
                    Me.sp               = SourcePoint.sp
                    Me.sh               = SourcePoint.sh
                    Me.CoordSys         = SourcePoint.CoordSys
                    Me.HeightSys        = SourcePoint.HeightSys
                    Me.StatusHints      = SourcePoint.StatusHints
                    
                    Me.SourcePath       = SourcePoint.SourcePath
                    Me.SourceLineNo     = SourcePoint.SourceLineNo
                    

                    ' Convert declared point type specific properties to attributes.
                    If (TypeOf SourcePoint Is GeoPoint) Then 
                        Me.AddPropertyAttributes(SourcePoint, Nothing, BindingFlags.DeclaredOnly Or BindingFlags.Public Or BindingFlags.Instance)
                    End If

                End If
            End Sub

            ''' <summary> Removes attributes representing properties, hence are listed in <see cref="PropertyAttributes"/>. </summary>
            Protected Sub RemovePropertyAttributes()
                For Each kvp As KeyValuePair(Of String, String) In PropertyAttributes
                    Dim AttributeName As String = kvp.Value
                    If (Me.Attributes.ContainsKey(AttributeName)) Then
                        Me.Attributes.Remove(AttributeName)
                    End If
                Next
            End Sub

            ''' <summary>
            ''' Adds attributes representing properties from <paramref name="SourcePoint"/>, 
            ''' which are declared in <see cref="PropertyAttributes"/>.
            ''' </summary>
             ''' <param name="SourcePoint">       Point to get property values and also the list of <see cref="PropertyAttributes"/> from. </param>
             ''' <param name="ExcludeProperties"> A list of properties that shouln't be added as attributes. May be <see langword="null"/>. </param>
             ''' <param name="PropertyFilter">    Determines, which properties of <paramref name="SourcePoint"/>  should be considered. </param>
             ''' <remarks>
             ''' <para>
             ''' The string attribute value will be converted from property value by it's <see cref="Object.ToString"/> method. 
             ''' An exception is a <see cref="Kilometer"/>, where the conversion is done by <see cref="Kilometer.ToKilometerNotation"/> 
             ''' with a precision set to 6.
             ''' </para>
             ''' <para>
             ''' <paramref name="ExcludeProperties"/>: The key is the property path. Items are dummies.
             ''' </para>
             ''' <para>
             ''' If an attribute already exists, it won't be changed.
             ''' </para>
             ''' </remarks>
            Public Sub AddPropertyAttributes(SourcePoint As GeoPoint, ExcludeProperties As Dictionary(Of String, String), PropertyFilter As BindingFlags)
                
                If (SourcePoint IsNot Nothing) Then
                    
                    For Each kvp As KeyValuePair(Of String, String) In SourcePoint.PropertyAttributes
                        
                        Dim PropertyPath  As String = kvp.key
                        Dim AttributeName As String = kvp.Value

                        If ((ExcludeProperties Is Nothing) OrElse (Not ExcludeProperties.ContainsKey(PropertyPath))) Then

                            Dim PropertyValue As Object = SourcePoint.GetPropertyValue(PropertyPath, PropertyFilter)
                            
                            If (PropertyValue IsNot Nothing) Then
                                If (Not Me.Attributes.ContainsKey(AttributeName)) Then
                                    Dim AttributeValue As String
                                    If (TypeOf PropertyValue Is Kilometer) Then
                                        AttributeValue = Sprintf("%s", DirectCast(PropertyValue, Kilometer).ToKilometerNotation(6, " "))
                                    Else
                                        AttributeValue = PropertyValue.ToString()
                                    End If

                                    Dim DoNotAdd As Boolean = False
                                    DoNotAdd = (DoNotAdd Or AttributeValue.IsEmptyOrWhiteSpace())  ' Value may be empty if PropertyValue is an object (i.e. Kilometer).
                                    DoNotAdd = (DoNotAdd Or (AttributeValue = "NaN"))
                                    DoNotAdd = (DoNotAdd Or ((AttributeName.ToLower() = Rstyx.Utilities.Resources.Messages.Domain_AttName_CoordType.ToLower()) AndAlso (AttributeValue = GeoIPoint.DefaultCoordType)))

                                    If (Not DoNotAdd) Then Me.Attributes.Add(AttributeName, AttributeValue)
                                End If
                            End If
                        End If
                    Next
                End If
            End Sub

            ''' <summary> Tries to convert all attributes declared in <see cref="PropertyAttributes"/> into matching properties. </summary>
             ''' <remarks>
             ''' <para>
             ''' The string attribute value will be converted into property value by "CType". 
             ''' If the conversion fails, the attribute won't be removed from <see cref="Attributes"/>,
             ''' and a warning will be logged.
             ''' </para>
             ''' <para>
             ''' Hint: At this time there is no way for the warnings to get into the <see cref="IO.GeoPointFile.ParseErrors"/>,
             ''' hence they can't get into jEdit's error-list.
             ''' </para>
             ''' </remarks>
            Public Sub ConvertPropertyAttributes()
                
                For Each kvp As KeyValuePair(Of String, String) In Me.PropertyAttributes
                    
                    Dim PropertyPath  As String = kvp.key
                    Dim AttributeName As String = kvp.Value
            
                    If (Me.Attributes.ContainsKey(AttributeName)) Then
                        
                        Dim AttributeValue As String = Me.Attributes(AttributeName)
                        Try
                            If (AttributeValue.IsNotEmptyOrWhiteSpace()) Then
                                Me.SetPropertyValue(PropertyPath, AttributeValue, BindingFlags.Public Or BindingFlags.Instance)
                            End If
                            Me.Attributes.Remove(AttributeName)

                        Catch ex As Exception
                            Logger.LogWarning(sprintf(Rstyx.Utilities.Resources.Messages.GeoPoint_InvalidAttributeValue, Me.ID, AttributeName, AttributeValue))
                        End Try
                    End If
                Next
            End Sub
            
        #End Region
        
        #Region "IIdentifiable Members"
            
            Protected _ID As String = Nothing
            
            ''' <summary> The point ID (It will be verified while setting it). </summary>
             ''' <remarks> The setter fails if given ID isn't valid for this point type. </remarks>
             ''' <exception cref="InvalidIDException"> The value to set as ID isn't a valid ID for this point. </exception>
            Public Property ID() As String Implements IIdentifiable(Of String).ID
                Get
                    Return _ID
                End Get
                Set(value As String)
                    _ID = Me.ParseID(value)
                End Set
            End Property
            
        #End Region
        
        #Region "IGeoPointInfo Members"
            
            Private _Attributes As Dictionary(Of String, String)
            
            ''' <summary> A bunch of free attributes (key/value pairs). </summary>
             ''' <remarks>
             ''' The Getter never returns <see langword="null"/> but rather an empty dictionary.
             ''' </remarks>
            Public Property Attributes()    As Dictionary(Of String, String) Implements IGeoPointInfo.Attributes
                Get
                    If (_Attributes Is Nothing) Then _Attributes = New Dictionary(Of String, String)
                    Return _Attributes
                End Get
            Set (value As Dictionary(Of String, String))
                _Attributes = value
            End Set
            End Property
            
            ''' <inheritdoc/>
            Public Property ActualCant()        As Double = Double.NaN Implements IGeoPointInfo.ActualCant
            
            ''' <inheritdoc/>
            Public Property ActualCantAbs()     As Double = Double.NaN Implements IGeoPointInfo.ActualCantAbs
            
            ''' <inheritdoc/>
            Public Property ActualTrackGauge()  As Double = Double.NaN Implements IGeoPointInfo.ActualTrackGauge
            
            ''' <inheritdoc/>
            Public Property Info()              As String = String.Empty Implements IGeoPointInfo.Info
            
            ''' <inheritdoc/>
            Public Property HeightInfo()        As String = String.Empty Implements IGeoPointInfo.HeightInfo
            
            ''' <inheritdoc/>
            Public Property Comment()           As String = String.Empty Implements IGeoPointInfo.Comment
            
            ''' <inheritdoc/>
            Public Property Kind()              As GeoPointKind = GeoPointKind.None Implements IGeoPointInfo.Kind
            
            ''' <inheritdoc/>
            Public Property KindText()          As String = String.Empty Implements IGeoPointInfo.KindText
            
            ''' <inheritdoc/>
            Public Property MarkType()          As String = String.Empty Implements IGeoPointInfo.MarkType
            
            ''' <inheritdoc/>
            Public Property MarkHints()         As String = String.Empty Implements IGeoPointInfo.MarkHints
            
            ''' <inheritdoc/>
            Public Property ObjectKey()         As String = String.Empty Implements IGeoPointInfo.ObjectKey
            
            ''' <inheritdoc/>
            Public Property Job()               As String = String.Empty Implements IGeoPointInfo.Job
            
            ''' <inheritdoc/>
            Public Property TimeStamp           As Nullable(Of DateTime) = Nothing Implements IGeoPointInfo.TimeStamp
            
            ''' <inheritdoc/>
            Public Property StatusHints()       As GeoPointStatusHints = GeoPointStatusHints.None Implements IGeoPointInfo.StatusHints
            
        #End Region
        
        #Region "ICartesianCoordinates3D Members"
            
            ''' <inheritdoc/>
            Public Property X()             As Double = Double.NaN   Implements ICartesianCoordinates3D.X
            
            ''' <inheritdoc/>
            Public Property Y()             As Double = Double.NaN   Implements ICartesianCoordinates3D.Y
            
            ''' <inheritdoc/>
            Public Property Z()             As Double = Double.NaN   Implements ICartesianCoordinates3D.Z
            
            ''' <inheritdoc/>
            Public Property mp()            As Double = Double.NaN   Implements ICartesianCoordinates3D.mp
                            
            ''' <inheritdoc/>
            Public Property mh()            As Double = Double.NaN   Implements ICartesianCoordinates3D.mh
                            
            ''' <inheritdoc/>
            Public Property wp()            As Double = Double.NaN   Implements ICartesianCoordinates3D.wp
                            
            ''' <inheritdoc/>
            Public Property wh()            As Double = Double.NaN   Implements ICartesianCoordinates3D.wh
                            
            ''' <inheritdoc/>
            Public Property sp()            As String = String.Empty Implements ICartesianCoordinates3D.sp
                            
            ''' <inheritdoc/>
            Public Property sh()            As String = String.Empty Implements ICartesianCoordinates3D.sh
            
            ''' <inheritdoc/>
            Public Property CoordSys()      As String = String.Empty Implements ICartesianCoordinates3D.CoordSys
            
            ''' <inheritdoc/>
            Public Property HeightSys()     As String = String.Empty Implements ICartesianCoordinates3D.HeightSys
            
        #End Region
        
        #Region "IFileSource Members"
            
            ''' <inheritdoc/>
            Public Property SourcePath()    As String = Nothing Implements IFileSource.SourcePath
            
            ''' <inheritdoc/>
            Public Property SourceLineNo()  As Long = 0 Implements IFileSource.SourceLineNo
            
        #End Region
        
        #Region "ID Validation"
            
            ''' <summary> Parses the given string as new <see cref="GeoPoint.ID"/> for this GeoPoint. </summary>
             ''' <param name="TargetID"> The string which is intended to become the new point ID. It will be trimmed here. </param>
             ''' <returns> The parsed ID. </returns>
             ''' <remarks>
             ''' <para>
             ''' This method will be called when setting the <see cref="GeoPoint.ID"/> property.
             ''' It allows for special format conversions and for validation of <paramref name="TargetID"/>.
             ''' </para>
             ''' <para>
             ''' For the base class <see cref="GeoPoint"/>, this method returns the trimmed <paramref name="TargetID"/>
             ''' and validation only complains about the ID, if it is <see langword="null"/>, empty or contains whitespace only.
             ''' Derived classes should override this behavior to implement more restrictive constraints.
             ''' </para>
             ''' </remarks>
             ''' <exception cref="InvalidIDException"> <paramref name="TargetID"/> isn't a valid ID for this point. </exception>
            Protected Overridable Function ParseID(TargetID As String) As String
                
                If (TargetID.IsEmptyOrWhiteSpace()) Then
                    Throw New InvalidIDException(Rstyx.Utilities.Resources.Messages.GeoPointConstraints_MissingPointID)
                End If
                
                Return TargetID.Trim()
            End Function
            
        #End Region
        
        #Region "Constraints Verifying"
            
            ''' <summary> Verifies that this point fulfills all given <paramref name="Constraints"/>. </summary>
             ''' <param name="Constraints"> A set of <see cref="GeoPointConstraints"/> to verify for this point. </param>
             ''' <remarks>
             ''' If any of the <see cref="GeoPointConstraints"/> is violated, a <see cref="ParseException"/> will be thrown.
             ''' In this case, a <see cref="ParseError"/> will be created and delivered with the <see cref="ParseException"/>.
             ''' The <see cref="ParseError"/> will contain error source information if available.
             ''' Therefore, <see cref="GeoPoint.SourceLineNo"/> and <see cref="GeoPoint.SourcePath"/> should be set for this point.
             ''' </remarks>
             ''' <exception cref="ParseException"> At least one constraint is violated. </exception>
            Public Sub VerifyConstraints(Constraints As GeoPointConstraints)
                Me.VerifyConstraints(Constraints, Nothing, Nothing, Nothing)
            End Sub
            
            ''' <summary> Verifies that this point fulfills all given <paramref name="Constraints"/>. </summary>
             ''' <param name="Constraints"> A set of <see cref="GeoPointConstraints"/> to verify for this point. </param>
             ''' <param name="FieldX">  The parsed data field of X coordinate. May be <see langword="null"/>. </param>
             ''' <param name="FieldY">  The parsed data field of Y coordinate. May be <see langword="null"/>. </param>
             ''' <param name="FieldZ">  The parsed data field of Z coordinate. May be <see langword="null"/>. </param>
             ''' <remarks>
             ''' If any of the <see cref="GeoPointConstraints"/> is violated, a <see cref="ParseException"/> will be thrown.
             ''' In this case, a <see cref="ParseError"/> will be created and delivered with the <see cref="ParseException"/>.
             ''' The <see cref="ParseError"/> will contain error source information if available.
             ''' Therefore, <see cref="GeoPoint.SourceLineNo"/> and <see cref="GeoPoint.SourcePath"/> should be set for this point.
             ''' The Datafields may be given in order to highlight violation position exactly in source file.
             ''' </remarks>
             ''' <exception cref="ParseException"> At least one constraint is violated. </exception>
            Public Sub VerifyConstraints(Constraints As GeoPointConstraints,
                                         FieldX      As DataField(Of Double),
                                         FieldY      As DataField(Of Double),
                                         FieldZ      As DataField(Of Double)
                                        )
                Dim PointID  As String  = Me.ID
                Dim StartCol As Integer = 0
                Dim EndCol   As Integer = 0
                
                ' Position missing.
                If (Constraints.HasFlag(GeoPointConstraints.KnownPosition)) Then
                    If (Double.IsNaN(Me.X) OrElse Double.IsNaN(Me.Y)) Then
                        If (Me.SourceLineNo > 0) Then
                            If ((FieldX IsNot Nothing) AndAlso (FieldY IsNot Nothing) AndAlso FieldX.HasSource AndAlso FieldY.HasSource) Then
                                If (FieldX.Source.Column < FieldY.Source.Column) Then
                                    StartCol = FieldX.Source.Column
                                    EndCol   = FieldY.Source.Column + FieldY.Source.Length
                                Else
                                    StartCol = FieldY.Source.Column
                                    EndCol   = FieldX.Source.Column + FieldX.Source.Length
                                End If
                            End If
                            Throw New ParseException(New ParseError(ParseErrorLevel.[Error],
                                                                    Me.SourceLineNo, StartCol, EndCol,
                                                                    Sprintf(Rstyx.Utilities.Resources.Messages.GeoPointConstraints_MissingPosition, PointID),
                                                                    Nothing,
                                                                    Me.SourcePath
                                                                   ))
                        Else
                            Throw New ParseException(New ParseError(ParseErrorLevel.[Error], Sprintf(Rstyx.Utilities.Resources.Messages.GeoPointConstraints_MissingPosition, PointID)))
                        End If
                    End If
                End If
                
                ' Heigt missing.
                If (Constraints.HasFlag(GeoPointConstraints.KnownHeight)) Then
                    If (Double.IsNaN(Me.Z)) Then
                        Throw New ParseException(ParseError.Create(ParseErrorLevel.[Error],
                                                                   Me.SourceLineNo,
                                                                   FieldZ,
                                                                   Sprintf(Rstyx.Utilities.Resources.Messages.GeoPointConstraints_MissingHeight, PointID),
                                                                   Nothing,
                                                                   Me.SourcePath
                                                                  ))
                    End If
                End If
                                    
                ' Canted Rails System missing.
                If (Constraints.HasFlag(GeoPointConstraints.KnownCantedRailsSystem)) Then
                    
                    Dim Missing As Boolean = False
                    Dim Hints   As String  = Nothing
                    
                    If (TypeOf Me IsNot IPointAtTrackGeometry) Then
                        Missing = True
                        Hints   = Rstyx.Utilities.Resources.Messages.GeoPointConstraints_Hint_MissingTrackValues
                    Else
                        Dim tp As GeoTcPoint = Me.AsGeoTcPoint()
                        
                        If (Double.IsNaN(tp.QG) OrElse Double.IsNaN(tp.HG)) Then
                            Missing = True
                            If (Double.IsNaN(tp.HSOK)) Then
                                Hints = Sprintf(Rstyx.Utilities.Resources.Messages.GeoPointConstraints_Hint_MissingField, Rstyx.Utilities.Resources.Messages.Domain_Label_HSOK)
                            ElseIf (Double.IsNaN(tp.Ueb)) Then
                                Hints = Sprintf(Rstyx.Utilities.Resources.Messages.GeoPointConstraints_Hint_MissingField, Rstyx.Utilities.Resources.Messages.Domain_Label_Ueb)
                            ElseIf (Double.IsNaN(tp.Ra)) Then
                                Hints = Sprintf(Rstyx.Utilities.Resources.Messages.GeoPointConstraints_Hint_MissingField, Rstyx.Utilities.Resources.Messages.Domain_Label_Ra)
                            ElseIf (tp.Ra.EqualsTolerance(0, 0.001)) Then
                                Hints = Rstyx.Utilities.Resources.Messages.GeoPointConstraints_Hint_MissingCantSign
                            End If
                        End If
                    End If
                    
                    If (Missing) Then
                        If (Me.SourceLineNo > 0) Then
                            Throw New ParseException(New ParseError(ParseErrorLevel.[Error],
                                                                    Me.SourceLineNo, 0, 0,
                                                                    Sprintf(Rstyx.Utilities.Resources.Messages.GeoPointConstraints_MissingCantedRailsSystem, PointID),
                                                                    Hints,
                                                                    Me.SourcePath
                                                                   ))
                        Else
                            Throw New ParseException(New ParseError(ParseErrorLevel.[Error], Sprintf(Rstyx.Utilities.Resources.Messages.GeoPointConstraints_MissingCantedRailsSystem, PointID)))
                        End If
                    End If
                End If
            End Sub
            
        #End Region
        
        #Region "Info Text - Parsing and Creating"
            
            ''' <summary> Creates a point info text for file output, containing cant (if any) and info. </summary>
             ''' <param name="Options"> Controls content of created text. </param>
             ''' <returns> The point info text for file output, i.e. 'u= 23  info'. </returns>
             ''' <remarks>
             ''' <para>
             ''' Depending on <paramref name="Options"/> special info will be added to pure info (<see cref="GeoPoint.Info"/>):
             ''' <list type="table">
             ''' <listheader> <term> <b>Option</b> </term>  <description> Result example </description></listheader>
             ''' <item> <term> <see cref="GeoPointOutputOptions.CreateInfoWithActualCant"/> </term>  <description> u= 23  info </description></item>
             ''' <item> <term> <see cref="GeoPointOutputOptions.CreateInfoWithPointKind "/> </term>  <description> LFP  info    </description></item>
             ''' </list>
             ''' </para>
             ''' </remarks>
            Public Overridable Function CreateInfoTextOutput(Options As GeoPointOutputOptions) As String
                
                Dim RetText As String = String.Empty
                
                ' Add actual rails cant.
                If (Options.HasFlag(GeoPointOutputOptions.CreateInfoWithActualCant)) Then
                    If ((Not Double.IsNaN(Me.ActualCant)) AndAlso (Not Double.IsNaN(Me.ActualCantAbs)) ) Then
                        RetText = Sprintf("u=%-3.0f ueb=%-3.0f %-s", Me.ActualCant * 1000, Me.ActualCantAbs * 1000 * -1, Me.Info)
                    ElseIf (Not Double.IsNaN(Me.ActualCant)) Then
                        RetText = Sprintf("u=%-3.0f %-s", Me.ActualCant * 1000, Me.Info)
                    ElseIf (Not Double.IsNaN(Me.ActualCantAbs)) Then
                        RetText = Sprintf("ueb=%-3.0f %-s", Me.ActualCantAbs * 1000 * -1, Me.Info)
                    End If
                End If
                
                ' Add kind info, unless there's already a point kind descriptor in there.
                If (RetText.IsEmpty() AndAlso (Not (Me.Kind = GeoPointKind.None)) AndAlso Options.HasFlag(GeoPointOutputOptions.CreateInfoWithPointKind)) Then
                    Dim AddKindText As Boolean = True
                    If (Me.Info.IsNotEmptyOrWhiteSpace()) Then
                        For Each kvp As KeyValuePair(Of String, GeoPointKind) In InfoKindPatterns
                            If (kvp.Value = Me.Kind) Then
                                Dim oMatch As Match = Regex.Match(Me.Info, kvp.Key, RegexOptions.IgnoreCase)
                                If (oMatch.Success) Then
                                    AddKindText = False
                                End If
                            End If
                        Next
                    End If
                    If (AddKindText) Then
                        RetText = Sprintf("%-4s %-s", Me.GetKindTextSmart(), Me.Info)
                    End If
                End If
                
                ' Add MarkTypeAB, if it isn't already part of Me.Info.
                If (RetText.IsEmpty() AndAlso (Not (Me.Kind = GeoPointKind.Rails)) AndAlso Options.HasFlag(GeoPointOutputOptions.CreateInfoWithPointKind)) Then
                    Dim cMarkTypeAB As String = Me.AsGeoIPoint.MarkTypeAB
                    If (cMarkTypeAB.IsNotEmptyOrWhiteSpace()) Then
                        Dim AddMarkTypeAB As Boolean = True
                        If (Me.Info.IsNotEmptyOrWhiteSpace()) Then
                            Dim oMatch As Match = Regex.Match(Me.Info, "^" & cMarkTypeAB & "\b", RegexOptions.IgnoreCase)
                            If (oMatch.Success) Then
                                AddMarkTypeAB = False
                            End If
                        End If
                        If (AddMarkTypeAB) Then
                            RetText = cMarkTypeAB & " " & Me.Info
                        End If
                    End If
                End If
                
                ' Set output text equal to original info text.
                If (RetText.IsEmpty()) Then
                    RetText = Me.Info
                End If
                
                Return RetText.TrimEnd()
            End Function
            
            ''' <summary>
            ''' <see cref="GeoPoint.Info"/> (and maybe <see cref="GeoPoint.Comment"/>) will be parsed for some info. 
            ''' The found values will be stored into point properties. 
            ''' </summary>
             ''' <param name="Options">  Controls what target info should be parsed for. </param>
             ''' <remarks>
             ''' <para>
             ''' If <see cref="GeoPoint.Kind"/> is already set when this method is invoked, there are two scenarios:
             ''' <list type="table">
             ''' <listheader> <term> <b>Point Kind</b> </term>  <description> Action </description></listheader>
             ''' <item> <term> Rails                   </term>  <description> Actual Cant will be parsed if it isn't known already (and <paramref name="Options"/> tells to do so) </description></item>
             ''' <item> <term> Other Kind              </term>  <description> This method does nothing </description></item>
             ''' </list>
             ''' </para>
             ''' <para>
             ''' The following patterns somewhere in <see cref="GeoPoint.Info"/> (or maybe in <see cref="GeoPoint.Comment"/>) lead to guessing the point kind: 
             ''' <list type="table">
             ''' <listheader> <term> <b>Pattern</b>                 </term>  <description> Point Kind                                            </description></listheader>
             ''' <item> <term> u *= *([+-]?([0-9]*[.])?[0-9]+)\s*   </term>  <description> Actual rails with actual cant                         </description></item>
             ''' <item> <term> ueb *= *([+-]?([0-9]*[.])?[0-9]+)\s* </term>  <description> Actual rails with actual absolute cant (sign as iGeo) </description></item>
             ''' <item> <term> Gls, Gleis                           </term>  <description> Actual rails (without actual cant)                    </description></item>
             ''' <item> <term> Bstg, Bst, Bahnst                    </term>  <description> Platform                                              </description></item>
             ''' <item> <term> PS4, GVP                             </term>  <description> Rails fix point                                       </description></item>
             ''' <item> <term> PS3, HFP, HB, HP                     </term>  <description> Fix point 1D                                          </description></item>
             ''' <item> <term> PS2, LFP, PPB, P[0-9]+               </term>  <description> Fix point 2D                                          </description></item>
             ''' <item> <term> PS1, GPSC, LHFP                      </term>  <description> Fix point 3D                                          </description></item>
             ''' <item> <term> PS0, NXO, DBRF                       </term>  <description> Fix point 3D                                          </description></item>
             ''' <item> <term> PP, AP, PSx                          </term>  <description> General fix point                                     </description></item>
             ''' <item> <term> SOK, SOK1, SOK2                      </term>  <description> Rail top (1, 2)                                       </description></item>
             ''' <item> <term> Messpkt, Messpkt1, Messpkt2          </term>  <description> Measure Point (1, 2)                                  </description></item>
             ''' </list>
             ''' </para>
             ''' <para>
             ''' When an actual rails point has been determined by a cant pattern, 
             ''' this cant pattern (inclusive trailing whitespace) will be removed from <see cref="GeoPoint.Info"/> 
             ''' (but not from <see cref="GeoPoint.Comment"/>), because it will be re-added to the point's InfoText 
             ''' when the point is written to a file. All other tokens are kept as they are.
             ''' </para>
             ''' <para>
             ''' This method changes the following properties:
             ''' <list type="table">
             ''' <listheader> <term> <b>Property</b> </term>  <description> Action </description></listheader>
             ''' <item> <term> <see cref="GeoPoint.Kind"/>          </term>  <description> Cleared and maybe set. </description></item>
             ''' <item> <term> <see cref="GeoPoint.ActualCantAbs"/> </term>  <description> Cleared and maybe set. </description></item>
             ''' <item> <term> <see cref="GeoPoint.ActualCant"/>    </term>  <description> Cleared and maybe set. </description></item>
             ''' <item> <term> <see cref="GeoPoint.Info"/>          </term>  <description> A found cant pattern will be removed. </description></item>
             ''' </list>
             ''' </para>
             ''' </remarks>
            Public Overridable Function ParseInfoTextInput(Options As GeoPointEditOptions) As ParseInfoTextResult
                
                Dim RetValue   As New ParseInfoTextResult()
                Dim TryComment As Boolean = Options.HasFlag(GeoPointEditOptions.ParseCommentToo)
                
                ' Parse for actual cant.
                If (Options.HasFlag(GeoPointEditOptions.ParseInfoForActualCant)) Then
                    
                    Dim IsUnknownCant As Boolean = (Double.IsNaN(Me.ActualCant) AndAlso Double.IsNaN(Me.ActualCantAbs))
                    Dim ParseCant     As Boolean = ((Me.Kind = GeoPointKind.None) OrElse ((Me.Kind = GeoPointKind.Rails) AndAlso IsUnknownCant))
                    
                    If (ParseCant) Then
                        RetValue = Me.ParseInfoForActualCant(TryComment:=TryComment)
                    End If
                End If
                
                ' Parse for point kind.
                If ((Me.Kind = GeoPointKind.None) AndAlso Options.HasFlag(GeoPointEditOptions.ParseInfoForPointKind)) Then
                    RetValue = Me.ParseInfoForPointKind(TryComment:=TryComment)
                End If
                
                ' RetValue is empty for both invoked methods...
                Return RetValue
            End Function
            
            ''' <summary> Guesses the point kind from <see cref="GeoPoint.Info"/> property (somewhat heuristic). </summary>
             ''' <param name="TryComment"> If <see langword="true"/> then <see cref="GeoPoint.Info"/> and <see cref="GeoPoint.Comment"/> will be parsed. </param>
             ''' <remarks>
             ''' <para>
             ''' The following patterns somewhere in <see cref="GeoPoint.Info"/> lead to guessing the point kind: 
             ''' <list type="table">
             ''' <listheader> <term> <b>Pattern</b>        </term>  <description> Point Kind                         </description></listheader>
             ''' <item> <term> Gls, Gleis                  </term>  <description> Actual rails (without actual cant) </description></item>
             ''' <item> <term> Bstg, Bst, Bahnst           </term>  <description> Platform                           </description></item>
             ''' <item> <term> PS4, GVP                    </term>  <description> Rails fix point                    </description></item>
             ''' <item> <term> PS3, HFP, HB, HP            </term>  <description> Fix point 1D                       </description></item>
             ''' <item> <term> PS2, LFP, PPB, P[0-9]+      </term>  <description> Fix point 2D                       </description></item>
             ''' <item> <term> PS1, GPSC, LHFP             </term>  <description> Fix point 3D                       </description></item>
             ''' <item> <term> PS0, NXO, DBRF              </term>  <description> Fix point 3D                       </description></item>
             ''' <item> <term> PP, AP, PSx                 </term>  <description> General fix point                  </description></item>
             ''' <item> <term> SOK, SOK1, SOK2             </term>  <description> Rail top (1, 2)                    </description></item>
             ''' <item> <term> Messpkt, Messpkt1, Messpkt2 </term>  <description> Measure Point (1, 2)               </description></item>
             ''' </list>
             ''' </para>
             ''' <para>
             ''' This method changes the following properties:
             ''' <list type="table">
             ''' <listheader> <term> <b>Property</b>       </term>  <description> Action </description></listheader>
             ''' <item> <term> <see cref="GeoPoint.Kind"/> </term>  <description> Cleared and maybe set. </description></item>
             ''' </list>
             ''' </para>
             ''' </remarks>
            Protected Function ParseInfoForPointKind(Optional TryComment As Boolean = False) As ParseInfoTextResult
                
                Me.Kind = GeoPointKind.None
                
                Dim i           As Integer = 0
                Dim SearchCount As Integer = If(TryComment, 2, 1)
                Dim SearchText  As String  = Me.Info
                Dim Success     As Boolean = False
                
                Do
                    i += 1
                    
                    If (SearchText.IsNotEmptyOrWhiteSpace()) Then
                        
                        ' Test patterns against SearchText (for patterns: see Shared Sub New()).
                        For Each kvp As KeyValuePair(Of String, GeoPointKind) In InfoKindPatterns
                            
                            Dim oMatch As Match = Regex.Match(SearchText, kvp.Key, RegexOptions.IgnoreCase)
                            
                            If (oMatch.Success) Then
                                Success = True
                                Me.Kind = kvp.Value
                                Exit For
                            End If
                        Next
                    End If
                    
                    SearchText = Me.Comment
                    
                Loop Until (Success OrElse (i = SearchCount))
                
                Return (New ParseInfoTextResult())
            End Function
            
            ''' <summary> Parses actual cant from <see cref="GeoPoint.Info"/> property. </summary>
             ''' <param name="TryComment"> If <see langword="true"/> and no cant has been found in <see cref="GeoPoint.Info"/>, the <see cref="GeoPoint.Comment"/> will be parsed, too. </param>
             ''' <remarks>
             ''' <para>
             ''' The cant is recognized by this pattern in <see cref="GeoPoint.Info"/>:  u *= *([+-]?([0-9]*[.])?[0-9]+)\s*
             ''' </para>
             ''' <para>
             ''' The absolute cant (sign as iGeo) is recognized by this pattern in <see cref="GeoPoint.Info"/>:  ueb *= *([+-]?([0-9]*[.])?[0-9]+)\s*
             ''' </para>
             ''' <para>
             ''' When cant has been found, 
             ''' this cant pattern (inclusive trailing whitespace) will be removed from <see cref="GeoPoint.Info"/> 
             ''' (but not from <see cref="GeoPoint.Comment"/>), because it will be re-added to the point's InfoText 
             ''' when the point is written to a file. All other tokens are kept as they are.
             ''' </para>
             ''' <para>
             ''' This method may change the following properties:
             ''' <list type="table">
             ''' <listheader> <term> <b>Property</b> </term>  <description> Action </description></listheader>
             ''' <item> <term> <see cref="GeoPoint.ActualCantAbs"/> </term>  <description> Cleared and maybe set. </description></item>
             ''' <item> <term> <see cref="GeoPoint.ActualCant"/>    </term>  <description> Cleared and maybe set. </description></item>
             ''' <item> <term> <see cref="GeoPoint.Info"/>          </term>  <description> A found cant pattern will be removed. </description></item>
             ''' <item> <term> <see cref="GeoPoint.Kind"/>          </term>  <description> Only if cant has been found, kind will be changed to <c>Rails</c>. </description></item>
             ''' </list>
             ''' </para>
             ''' </remarks>
            Protected Function ParseInfoForActualCant(Optional TryComment As Boolean = False) As ParseInfoTextResult
                
                Me.ActualCant    = Double.NaN
                Me.ActualCantAbs = Double.NaN
                
                Dim i           As Integer = 0
                Dim SearchCount As Integer = If(TryComment, 2, 1)
                Dim SearchText  As String  = Me.Info
                Dim Success     As Boolean = False
                
                Do
                    i += 1
                    
                    If (SearchText.IsNotEmptyOrWhiteSpace()) Then
                        
                        ' Test EVERY pattern against SearchText, since both "ueb=" and "u=" may be there (for patterns: see Shared Sub New()).
                        For Each kvp As KeyValuePair(Of String, String) In InfoCantPatterns
                            
                            Dim oMatch As Match = Regex.Match(SearchText, kvp.Key, RegexOptions.IgnoreCase)
                            
                            If (oMatch.Success) Then
                                Success = True
                                Me.Kind = GeoPointKind.Rails
                                
                                ' Set actual cant and remove cant pattern from SearchText.
                                If (oMatch.Groups.Count > 2) Then
                                    
                                    Select Case oMatch.Groups(1).Value.ToLower()
                                        Case "ueb":  Me.ActualCantAbs = -1 * CDbl(oMatch.Groups(2).Value.Replace(" ", String.Empty)) / 1000
                                        Case "u":    Me.ActualCant    =      CDbl(oMatch.Groups(2).Value.Replace(" ", String.Empty)) / 1000
                                    End Select
                                    
                                    If (i = 1) Then
                                        SearchText = SearchText.Remove(oMatch.Index, oMatch.Length).TrimEnd()
                                    Else
                                        ' Leave Me.Comment unchanged.
                                    End If
                                End If
                            End If
                        Next
                        If (i = 1) Then
                            Me.Info = SearchText
                        Else
                            ' Leave Me.Comment unchanged.
                        End If
                    End If
                    
                    SearchText = Me.Comment
                    
                Loop Until (Success OrElse (i = SearchCount))
                
                Return (New ParseInfoTextResult())
            End Function
            
        #End Region
        
        #Region "Misc"
            
            ''' <summary>
             ''' Gets the value of an attribute which name is determined by the given property name
             ''' and the <see cref="PropertyAttributes"/> assignment table.
             ''' </summary>
             ''' <param name="PropertyPath"> Path to the property name, i.e. "prop1" or "prop1.prop2.prop3". May be <see langword="null"/>. </param>
             ''' <returns> The attribute's string value. May be <see langword="null"/>. </returns>
             ''' <remarks>
             ''' If <paramref name="PropertyPath"/> is a key in <see cref="PropertyAttributes"/>, the matching dictionary value
             ''' is the attribute name to look for. If this attribute exists in <see cref="Attributes"/>, 
             ''' it's value will be returned.
             ''' </remarks>
            Public Function GetAttValueByPropertyName(PropertyPath As String) As String
                
                Dim AttValue As String = Nothing
                
                If (PropertyPath.IsNotEmptyOrWhiteSpace()) Then
                    If (Me.PropertyAttributes.ContainsKey(PropertyPath)) Then
                        Dim AttName As String = Me.PropertyAttributes(PropertyPath)
                        If (AttName.IsNotEmptyOrWhiteSpace()) Then
                            If (Me.Attributes.ContainsKey(AttName)) Then
                                AttValue = Me.Attributes(AttName)
                            End If
                        End If
                    End If
                End If
                
                Return AttValue
            End Function
            
            ''' <summary> Tries to set <see cref="GeoPoint.Kind"/> according to <see cref="GeoPoint.KindText"/>. </summary>
             ''' <remarks>
             ''' <para>
             ''' There are some internal mappings for this.
             ''' </para>
             ''' <para>
             ''' This method changes the following properties:
             ''' <list type="table">
             ''' <listheader> <term> <b>Property</b> </term>  <description> Action </description></listheader>
             ''' <item> <term> <see cref="GeoPoint.Kind"/> </term>  <description> Reset to <c>None</c> and maybe set. </description></item>
             ''' </list>
             ''' </para>
             ''' </remarks>
            Public Sub SetKindFromKindText()
                If (Me.KindText.IsNotEmptyOrWhiteSpace() AndAlso KindText2Kind.ContainsKey(Me.KindText)) Then
                    Me.Kind = KindText2Kind(Me.KindText)
                Else
                    Me.Kind = GeoPointKind.None
                End If
            End Sub
            
            ''' <summary> Tries to set <see cref="GeoPoint.Kind"/> according to <see cref="GeoPoint.MarkType"/>. </summary>
             ''' <remarks>
             ''' <para>
             ''' There are some internal mappings for this from GND-Edit specification.
             ''' </para>
             ''' <para>
             ''' This method changes the following properties:
             ''' <list type="table">
             ''' <listheader> <term> <b>Property</b> </term>  <description> Action </description></listheader>
             ''' <item> <term> <see cref="GeoPoint.Kind"/> </term>  <description> Reset to <c>None</c> and maybe set. </description></item>
             ''' </list>
             ''' </para>
             ''' </remarks>
            Public Sub SetKindFromMarkType()
                If (Me.MarkType.IsNotEmptyOrWhiteSpace() AndAlso MarkType2Kind.ContainsKey(Me.MarkType)) Then
                    Me.Kind = MarkType2Kind(Me.MarkType)
                Else
                    Me.Kind = GeoPointKind.None
                End If
            End Sub
            
            ''' <summary> Gets stored <see cref="GeoPoint.KindText"/> if not empty, otherwise a default kind text for <see cref="GeoPoint.Kind"/>. </summary>
             ''' <remarks>
             ''' <para>
             ''' There are internal mappings for this.
             ''' </para>
             ''' </remarks>
            Public Function GetKindTextSmart() As String
                Dim RetValue As String = Me.KindText
                If (RetValue.IsEmptyOrWhiteSpace() AndAlso (Me.Kind <> GeoPointKind.None)) Then
                    RetValue = DefaultKindText(Me.Kind)
                End If
                Return RetValue
            End Function
            
            ''' <summary> Gets a List of associations: MarkType => PointKind. </summary>
             ''' <returns> List of associations: MarkType => PointKind </returns>
            Public Function GetListMarkType2Kind() As String
                Dim RetValue As String = Rstyx.Utilities.Resources.Messages.GeoPoint_MarkKindListHeader
                For Each kvp As KeyValuePair(Of String, GeoPointKind) in MarkType2Kind
                    RetValue &= Sprintf(Rstyx.Utilities.Resources.Messages.GeoPoint_MarkKindListRow, kvp.Key, kvp.Value.ToDisplayString())
                Next
                Return RetValue
            End Function
            
        #End Region
            
        #Region "Overrides"
            
            ''' <summary> Returns point ID and info. </summary>
            Public Overrides Function ToString() As String
                Return Me.ID & "  (" & Me.Info & ")"
            End Function
            
        #End Region

        #Region "Nested Classes"
            
            ''' <summary> Result state of <see cref="ParseInfoTextInput(GeoPointEditOptions)"/> </summary>
            Public Class ParseInfoTextResult
                
                ''' <summary> Tells if there has been a conflict applying a parsed value. </summary>
                Public HasConflict  As Boolean = False
                
                ''' <summary> A message for a <see cref="ParseError"/> if <see cref="HasConflict"/> is <see langword="true"/>. </summary>
                Public Message      As String = Nothing
                
                ''' <summary> Hints for a <see cref="ParseError"/> if <see cref="HasConflict"/> is <see langword="true"/>. </summary>
                Public Hints        As String = Nothing
                
            End Class
            
        #End Region
        
    End Class

End Namespace

' for jEdit:  :collapseFolds=2::tabSize=4::indentSize=4:
