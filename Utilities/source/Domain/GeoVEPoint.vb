
Imports System
Imports System.Collections.Generic
Imports System.Math

Imports Rstyx.Utilities.Domain
Imports Rstyx.Utilities.IO
Imports Rstyx.Utilities.StringUtils

Namespace Domain
    
    ''' <summary> One single VE point. </summary>
    ''' <remarks>
    ''' The <see cref="GeoPoint.ID"/> is maintained as String representation of an integer with a configurable maximum length.
    ''' </remarks>
    Public Class GeoVEPoint
        Inherits GeoPoint
        
        #Region "Private Fields"
            
            'Private Shared Logger As Rstyx.LoggingConsole.Logger = Rstyx.LoggingConsole.LogBox.getLogger("Rstyx.Utilities.Domain.GeoVEPoint")
            
        #End Region
        
        #Region "ID Definition Fields"
            
            ''' <summary> Gets the default maximum length of <see cref="GeoVEPoint.ID"/>. </summary>
            Public Shared ReadOnly DefaultMaxIDLength As Integer = 7
            
            ''' <summary> Gets the maximum length of <see cref="GeoVEPoint.ID"/> as determined at instantiation. </summary>
            Public ReadOnly MaxIDLength   As Integer
            
            ''' <summary> Gets the factor that converts a double <see cref="GeoVEPoint.ID"/> into an integer one (derived from <see cref="GeoVEPoint.MaxIDLength"/>). </summary>
            Public ReadOnly PointNoFactor As Integer
            
            ''' <summary> Gets the minimal integer <see cref="GeoVEPoint.ID"/> (always <b>1</b>). </summary>
            Public ReadOnly MinIntegerID  As Integer
            
            ''' <summary> Gets the maximum integer <see cref="GeoVEPoint.ID"/> (derived from <see cref="GeoVEPoint.MaxIDLength"/>). </summary>
            Public ReadOnly MaxIntegerID  As Integer
            
        #End Region
        
        #Region "Constuctor"
            
            ''' <summary> Creates a new, empty <see cref="GeoVEPoint"/> with a <see cref="GeoVEPoint.MaxIDLength"/> of <see cref="GeoVEPoint.DefaultMaxIDLength"/>. </summary>
            Public Sub New()
                Me.MaxIDLength   = DefaultMaxIDLength
                Me.PointNoFactor = Pow(10, MaxIDLength - 2)
                Me.MinIntegerID  = 1
                Me.MaxIntegerID  = Pow(10, MaxIDLength) - 1
            End Sub
            
            ''' <summary> Creates a new, empty <see cref="GeoVEPoint"/> with a given Value for <see cref="GeoVEPoint.MaxIDLength"/>. </summary>
             ''' <param name="MaxIDLength"> The maximum supported length resp. digits for the point ID. </param>
             ''' <remarks>  </remarks>
             ''' <exception cref="System.ArgumentOutOfRangeException"> <paramref name="MaxIDLength"/> doesn't equals 6 or 7. </exception>
            Public Sub New(MaxIDLength As Integer)
                
                If ((MaxIDLength < 6) OrElse (MaxIDLength > 7)) Then Throw New ArgumentOutOfRangeException("MaxIDLength")
                
                Me.MaxIDLength   = MaxIDLength
                Me.PointNoFactor = Pow(10, MaxIDLength - 2)
                Me.MinIntegerID  = 1
                Me.MaxIntegerID  = Pow(10, MaxIDLength) - 1
            End Sub
            
            ''' <summary> Creates a new <see cref="GeoVEPoint"/> with a <see cref="GeoVEPoint.MaxIDLength"/> of
            ''' <see cref="GeoVEPoint.DefaultMaxIDLength"/> and inititializes it's properties from a given <see cref="IGeoPoint"/>.
            ''' </summary>
             ''' <param name="SourcePoint"> The source point to get init values from. May be <see langword="null"/>. </param>
             ''' <remarks>
             ''' <para>
             ''' If <paramref name="SourcePoint"/> is a <see cref="GeoVEPoint"/>, then all properties will be taken.
             ''' </para>
             ''' <para>
             ''' If <paramref name="SourcePoint"/> is a <see cref="GeoTCPoint"/>, then the following properties will be taken:
             ''' <list type="table">
             ''' <listheader> <term> <b>GeoVEPoint Property Name</b> </term>  <description> <b>GeoTCPoint Property Name</b> </description></listheader>
             ''' <item> <term> TrackPos.Kilometer   </term>  <description> Km </description></item>
             ''' </list>
             ''' </para>
             ''' <para>
             ''' If <paramref name="SourcePoint"/> is a <see cref="GeoIPoint"/>, then the following properties will be converted to attributes:
             ''' <list type="table">
             ''' <listheader> <term> <b>Property Name</b> </term>  <description> <b>Attribute Name</b> </description></listheader>
             ''' <item> <term> MarkTypeAB </term>  <description> VArtAB </description></item>
             ''' </list>
             ''' </para>
             ''' <para>
             ''' The following attributes will be converted to properties, if the properties have no value yet:
             ''' <list type="table">
             ''' <listheader> <term> <b>Attribute Name</b> </term>  <description> <b>Property Name</b> </description></listheader>
             ''' <item> <term> StrNr </term>  <description> TrackPos.TrackNo   </description></item>
             ''' <item> <term> StrRi </term>  <description> TrackPos.RailsCode </description></item>
             ''' <item> <term> StrKm </term>  <description> TrackPos.Kilometer </description></item>
             ''' </list>
             ''' </para>
             ''' </remarks>
             ''' <exception cref="InvalidIDException"> ID of <paramref name="SourcePoint"/> isn't a valid ID for this point. </exception>
            Public Sub New(SourcePoint As IGeoPoint)
                
                Me.New()
                Me.GetPropsFromIGeoPoint(SourcePoint)
                
                If (TypeOf SourcePoint Is GeoVEPoint) Then
                    
                    Dim SourceVEPoint As GeoVEPoint = DirectCast(SourcePoint, GeoVEPoint)
                    
                    Me.HeightPostInfo   = SourceVEPoint.HeightPostInfo
                    Me.HeightPreInfo    = SourceVEPoint.HeightPreInfo
                    Me.PositionPostInfo = SourceVEPoint.PositionPostInfo
                    Me.PositionPreInfo  = SourceVEPoint.PositionPreInfo
                    Me.TrackPos         = SourceVEPoint.TrackPos
                    
                Else
                    Dim PropertyName   As String
                    Dim AttributeName  As String
                    Dim AttStringValue As String
                    Dim AttIntValue    As Integer
                    
                    ' Source is TC:  Take compatible properties.
                    If (TypeOf SourcePoint Is GeoTcPoint) Then
                        
                        Dim SourceTcPoint As GeoTcPoint = DirectCast(SourcePoint, GeoTcPoint)
                        
                        Me.TrackPos.Kilometer = SourceTcPoint.Km
                    End If
                    
                    
                    ' Source is ipkt:  Convert ipkt-unique properties to attributes.
                    If (TypeOf SourcePoint Is GeoIPoint) Then
                        
                        Dim SourceIPoint As GeoIPoint = DirectCast(SourcePoint, GeoIPoint)
                        
                        PropertyName = "MarkTypeAB"
                        If (SourceIPoint.MarkTypeAB.IsNotEmpty()) Then
                            AttributeName = AttributeNames(PropertyName) 
                            If (Not Attributes.ContainsKey(AttributeName)) Then
                                Attributes.Add(AttributeName, SourceIPoint.MarkTypeAB)
                            End If
                        End If
                    End If
                    
                    
                    ' Convert selected attributes to properties.
                    PropertyName = "TrackPos.TrackNo"
                    If (Me.TrackPos.TrackNo Is Nothing) Then
                        AttStringValue = GetAttValueByPropertyName(PropertyName)
                        If (AttStringValue IsNot Nothing) Then
                            If (Integer.TryParse(AttStringValue, AttIntValue)) Then
                                Me.TrackPos.TrackNo = AttIntValue
                            End If
                            Attributes.Remove(AttributeNames(PropertyName))
                        End If
                    End If
                    
                    PropertyName = "TrackPos.RailsCode"
                    If (Me.TrackPos.RailsCode.IsEmptyOrWhiteSpace()) Then
                        AttStringValue = GetAttValueByPropertyName(PropertyName)
                        If (AttStringValue IsNot Nothing) Then
                            AttStringValue = AttStringValue.Trim()
                            If (AttStringValue.Length = 1) Then
                                Me.TrackPos.RailsCode = AttStringValue
                            End If
                            Attributes.Remove(AttributeNames(PropertyName))
                        End If
                    End If
                    
                    PropertyName = "TrackPos.Kilometer"
                    If (Not Me.TrackPos.Kilometer.HasValue()) Then
                        AttStringValue = GetAttValueByPropertyName(PropertyName)
                        If (AttStringValue IsNot Nothing) Then
                            Me.TrackPos.Kilometer.TryParse(AttStringValue)
                            Attributes.Remove(AttributeNames(PropertyName))
                        End If
                    End If
                    
                End If
            End Sub
            
        #End Region
        
        #Region "Properties"
            
            ''' <summary> General position description related to a railway track. </summary>
            Public Property TrackPos()          As PositionAtTrack = New PositionAtTrack()
            
            ''' <summary> Pre character of position info. </summary>
            Public Property PositionPreInfo     As Char = " "c
            
            ''' <summary> Post character of position info. </summary>
            Public Property PositionPostInfo    As Char = " "c
            
            ''' <summary> Pre character of height info. </summary>
            Public Property HeightPreInfo       As Char = " "c
            
            ''' <summary> Post character of height info. </summary>
            Public Property HeightPostInfo      As Char = " "c
            
        #End Region
                
        #Region "Instance Methods"
            
            ''' <summary> Returns the KEY part of <see cref="ID"/>. </summary>
             ''' <returns> The KEY part of <see cref="ID"/>. </returns>
             ''' <remarks>
             ''' The KEY part is the integer part of the double representation of ID.
             ''' I.e: ID = "1234567"  => key = 12 (when <see cref="PointNoFactor"/> is 5).
             ''' </remarks>
            Public Function GetKeyOfID() As Byte
                Return If(Me.ID.IsEmptyOrWhiteSpace(), 0, CByte(Int(CDbl(Me.ID) / PointNoFactor)))
            End Function
            
            
            ''' <summary> Gets a Double for VE flavoured storage from this point's ID. </summary>
             ''' <returns> Me.ID / PointNoFactor or 1.0E+40 if ID is still <see langword="null"/>. </returns>
            Public Function IDToVEDouble() As Double
                Return If(Me.ID.IsEmptyOrWhiteSpace(), 1.0E+40, CDbl(Me.ID) / PointNoFactor)
            End Function
            
            ''' <summary> Gets a Single for VE flavoured storage from this point's ID. </summary>
             ''' <returns> Me.ID / PointNoFactor or Zero if ID is still <see langword="null"/>. </returns>
            Public Function IDToVESingle() As Single
                Return If(Me.ID.IsEmptyOrWhiteSpace(), 0.0, CSng(Me.ID) / PointNoFactor)
            End Function
            
            ''' <summary> Gets an Int32 for VE flavoured storage from this point's ID. </summary>
             ''' <returns> Me.ID or Zero if ID is still <see langword="null"/>. </returns>
            Public Function IDToVEInt32() As Int32
                Return If(Me.ID.IsEmptyOrWhiteSpace(), 0, CInt(Me.ID))
            End Function
            
            
            ''' <summary> Validates a given integer ID. </summary>
             ''' <param name="IntegerID"> The ID to validate </param>
             ''' <returns> <see langword="true"/>, if <paramref name="IntegerID"/> could be applied as <see cref="GeoVEPoint.ID"/>. </returns>
            Public Function IsValidID(IntegerID As Integer) As Boolean
                Return ((Me.MinIntegerID <= IntegerID) AndAlso (IntegerID <= Me.MaxIntegerID))
            End Function
            
            ''' <summary> Validates a given double ID. </summary>
             ''' <param name="DoubleID"> The ID to validate </param>
             ''' <returns> <see langword="true"/>, if <paramref name="DoubleID"/> could be applied as <see cref="GeoVEPoint.ID"/>. </returns>
            Public Function IsValidID(DoubleID As Double) As Boolean
                
                Dim MinDoubleID As Double = Me.MinIntegerID / Me.PointNoFactor
                Dim MaxDoubleID As Double = Me.MaxIntegerID / Me.PointNoFactor
                Dim ModDoubleID As Double = Round(DoubleID, Me.MaxIDLength)
                
                Return ((MinDoubleID <= ModDoubleID) AndAlso (ModDoubleID <= MaxDoubleID))
            End Function
            
            ''' <summary> Validates a given single ID. </summary>
             ''' <param name="SingleID"> The ID to validate </param>
             ''' <returns> <see langword="true"/>, if <paramref name="SingleID"/> could be applied as <see cref="GeoVEPoint.ID"/>. </returns>
            Public Function IsValidID(SingleID As Single) As Boolean
                Return IsValidID(CDbl(SingleID))
            End Function
            
            
            ''' <summary> Gets a decimal string representation from this point's ID. </summary>
             ''' <returns> I.e. "12.34567", "1.23000" , or an empty string if ID is still <see langword="null"/>. </returns>
            Public Function FormatID() As String
                Return If(Me.ID.IsEmptyOrWhiteSpace(), String.Empty, sprintf("%." & CStr(MaxIDLength - 2) & "f", CDbl(Me.ID) / PointNoFactor))
            End Function
            
            
            ''' <summary> Parses the given Double as new <see cref="GeoVEPoint.ID"/> for this GeoVEPoint. </summary>
             ''' <param name="TargetID"> The Integer which is intended to become the new point ID. </param>
             ''' <remarks>
             ''' <para>
             ''' Example: Input = 123456  => ID = "123456"
             ''' </para>
             ''' <para>
             ''' Use this method if the ID to set isn't already a string but an integer.
             ''' </para>
             ''' <para>
             ''' This method is a foolproof and more performat alternative to setting the ID property directly.
             ''' </para>
             ''' </remarks>
             ''' <exception cref="InvalidIDException"> <paramref name="TargetID"/> isn't a valid ID for this point - see above. </exception>
            Public Overloads Sub ParseID(TargetID As Integer)
                If (Not Me.IsValidID(TargetID)) Then
                    Throw New InvalidIDException(sprintf(Rstyx.Utilities.Resources.Messages.GeoPointConstraints_IDOutOfIntRange, TargetID.ToString(), MinIntegerID, MaxIntegerID))
                End If
                _ID = TargetID.ToString()
            End Sub
            
            ''' <summary> Parses the given Double as new <see cref="GeoVEPoint.ID"/> for this GeoVEPoint. </summary>
             ''' <param name="TargetID"> The Double which is intended to become the new point ID. </param>
             ''' <remarks>
             ''' <para>
             ''' Example: Input = 12.3456789  => ID = "1234568"
             ''' </para>
             ''' <para>
             ''' Use this method if the ID to set isn't already a string but a Double,
             ''' because there are traps otherwise.
             ''' </para>
             ''' <para>
             ''' This method is a foolproof and more performat alternative to setting the ID property directly.
             ''' </para>
             ''' </remarks>
             ''' <exception cref="InvalidIDException"> <paramref name="TargetID"/> isn't a valid ID for this point - see above. </exception>
            Public Overloads Sub ParseID(TargetID As Double)
                If (Not Me.IsValidID(TargetID)) Then
                    Throw New InvalidIDException(sprintf(Rstyx.Utilities.Resources.Messages.GeoPointConstraints_IDOutOfIntRange, TargetID.ToString(), MinIntegerID, MaxIntegerID))
                End If
                _ID = sprintf("%.0f", TargetID * PointNoFactor)
            End Sub
            
            ''' <summary> Tries to set unknown <see cref="GeoPoint.Kind"/> according to <see cref="GeoPoint.MarkType"/>. </summary>
             ''' <remarks>
             ''' <para>
             ''' If <see cref="GeoPoint.Kind"/> is <c>None</c> and <see cref="GeoPoint.MarkType"/> is not emty or zero, 
             ''' then <see cref="GeoPoint.Kind"/> will be set to <c>FixPoint</c>
             ''' </para>
             ''' <para>
             ''' This method may change the following properties:
             ''' <list type="table">
             ''' <listheader> <term> <b>Property</b> </term>  <description> Action </description></listheader>
             ''' <item> <term> <see cref="GeoPoint.Kind"/> </term>  <description> Maybe changed. </description></item>
             ''' <item> <term> <see cref="GeoPoint.KindText"/> </term>  <description> Maybe changed. </description></item>
             ''' </list>
             ''' </para>
             ''' <para>
             ''' <see cref="GeoPoint.Kind"/> is set due to these rules:
             ''' <list type="table">
             ''' <listheader> <term> <b>MarkType Value </b> </term>  <description> <b> Kind Value </b> </description></listheader>
             ''' <item> <term> empty or zero </term>  <description> no change </description></item>
             ''' <item> <term> 1, 2, 3, 4    </term>  <description> <c>RailsFixPoint</c> </description></item>
             ''' <item> <term> other value   </term>  <description> <c>FixPoint</c> </description></item>
             ''' </list>
             ''' </para>
             ''' </remarks>
            Public Sub SetKindFromMarkType()
                If (Me.Kind = GeoPointKind.None) Then
                    If (Me.MarkType.IsNotEmptyOrWhiteSpace() AndAlso (Not (Me.MarkType = "0"))) Then
                        
                        Me.Kind = GeoPointKind.FixPoint
                        
                        Dim MarkTypeInt As Integer
                        If (Integer.TryParse(Me.MarkType, MarkTypeInt)) Then
                            If (MarkTypeInt < 5) Then
                                Me.Kind = GeoPointKind.RailsFixPoint
                            End If
                        End If
                        
                        Me.SetKindTextFromKind(Override:=True)
                    End If
                End If
            End Sub
            
            ''' <summary> Creates a point info text of max. 13 chars for file output, containing cant (if any) and info. </summary>
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
            Public Overrides Function CreateInfoTextOutput(Options As GeoPointOutputOptions) As String
                Return MyBase.CreateInfoTextOutput(Options).TrimToMaxLength(13)
            End Function
            
            ''' <summary>
            ''' <see cref="GeoPoint.Info"/> (and maybe <see cref="GeoPoint.Comment"/>) will be parsed for some info. 
            ''' The found values will be stored into point properties. 
            ''' </summary>
             ''' <param name="TryComment"> If <see langword="true"/> then <see cref="GeoPoint.Info"/> and <see cref="GeoPoint.Comment"/> will be parsed. </param>
             ''' <param name="Options">  Controls what target info should be parsed for. </param>
             ''' <remarks>
             ''' <para>
             ''' See <see cref="GeoPoint.ParseInfoTextInput"/> for details.
             ''' </para>
             ''' <para>
             ''' <see cref="GeoVEPoint"/> special: If point kind field is "Gls" => only try to get actual cant.
             ''' </para>
             ''' </remarks>
            Public Overrides Function ParseInfoTextInput(Options As GeoPointEditOptions, Optional TryComment As Boolean = False) As ParseInfoTextResult
                
                Dim RetValue As New ParseInfoTextResult()
                
                ' VermEsn point kind field is "Gls" => try to get actual cant.
                If (Me.Kind = GeoPointKind.Rails) Then
                    RetValue = Me.ParseInfoForActualCant(TryComment:=TryComment)
                End If
                
                ' Standard kind guessing.
                If (Me.Kind = GeoPointKind.None) Then
                   RetValue = MyBase.ParseInfoTextInput(Options:=Options, TryComment:=TryComment)
                End If
                
                Return RetValue
            End Function
            
            ''' <summary> Creates a point info text of max. 13 chars for kv file, containing cant (if any) and info. </summary>
             ''' <returns> The point info text for kv file, i.e. 'u= 23  info'. </returns>
             ''' <remarks>
             ''' DEPRECATED:  Use <see cref="GeoPoint.CreateInfoTextOutput(GeoPointOutputOptions)"/>.
             ''' </remarks>
            Public Function OLD_CreateKVInfo() As String
                
                Dim KVText As String
                
                If (Not Double.IsNaN(Me.ActualCant)) Then
                    If (Me.Info.Length < 7) Then
                        KVText = sprintf("u=%3.0f  %-s", Me.ActualCant * 1000, Me.Info)
                    ElseIf (Me.Info.Length = 7) Then
                        KVText = sprintf("u=%3.0f %-s", Me.ActualCant * 1000, Me.Info)
                    ElseIf (Me.Info.Length = 8) Then
                        KVText = sprintf("u=%2.0f %-s", Me.ActualCant * 1000, Me.Info)
                    Else
                        KVText = sprintf("u=%.0f %-s", Me.ActualCant * 1000, Me.Info)
                    End If
                Else
                    KVText = Me.Info
                End If
                
                Return KVText.TrimToMaxLength(13)
            End Function
            
        #End Region
        
        #Region "Static Methods"
            
            ''' <summary> Gets the usual (decimal) VE notation from a given ID. </summary>
             ''' <param name="ID"> The point ID to format. May be <see langword="null"/>. </param>
             ''' <returns> I.e. "12.34567", "1.23000" , or an empty string if ID is <see langword="null"/>. </returns>
             ''' <exception cref="InvalidIDException"> <paramref name="ID"/> isn't a valid ID for this point. </exception>
            Public Shared Function FormatID(ID As String) As String
                Dim RetID As String = ID
                If (ID.IsNotEmptyOrWhiteSpace()) Then
                    Dim VEPoint As New GeoVEPoint()
                    VEPoint.ID = ID
                    RetID = VEPoint.FormatID()
                End If
                Return RetID
            End Function
            
            ''' <summary> Gets the usual (decimal) VE notation from a given ID. </summary>
             ''' <param name="ID"> The point ID to format. May be <see langword="null"/>. </param>
             ''' <param name="MaxIDLength"> The maximum supported length resp. digits for the point ID. </param>
             ''' <returns> I.e. "12.34567", "1.23000" , or an empty string if ID is <see langword="null"/>. </returns>
             ''' <exception cref="System.ArgumentOutOfRangeException"> <paramref name="MaxIDLength"/> doesn't equals 6 or 7. </exception>
             ''' <exception cref="InvalidIDException"> <paramref name="ID"/> isn't a valid ID for this point. </exception>
            Public Shared Function FormatID(ID As String, MaxIDLength As Integer) As String
                Dim RetID As String = ID
                If (ID.IsNotEmptyOrWhiteSpace()) Then
                    Dim VEPoint As New GeoVEPoint(MaxIDLength)
                    VEPoint.ID = ID
                    RetID = VEPoint.FormatID()
                End If
                Return RetID
            End Function
            
        #End Region
        
        #Region "Overrides"
            
            ''' <summary> Parses the given string as new <see cref="GeoPoint.ID"/> for this GeoPoint. </summary>
             ''' <param name="TargetID"> The string which is intended to become the new point ID. It will be trimmed here. </param>
             ''' <returns> The parsed ID. </returns>
             ''' <remarks>
             ''' <para>
             ''' This method will be called when setting the <see cref="GeoPoint.ID"/> property.
             ''' It allows for special format conversions and for validation of <paramref name="TargetID"/>.
             ''' </para>
             ''' <para>
             ''' There are <b>two supported formats</b> for the point ID:
             ''' <list type="bullet">
             ''' <item><description> Always Positive numeric </description></item>
             ''' <item><description> If ID contains a ".", then valid range is: 0.00001 - 99.99999 </description></item>
             ''' <item><description> If ID doesn't contain a ".", then valid range is: 1 - 9999999 </description></item>
             ''' <item><description>  </description></item>
             ''' </list>
             ''' </para>
             ''' </remarks>
             ''' <exception cref="InvalidIDException"> <paramref name="TargetID"/> isn't a valid ID for this point - see above. </exception>
            Protected Overrides Function ParseID(TargetID As String) As String
                
                Dim RetValue As String  = MyBase.ParseID(TargetID)
                Dim DoubleID As Double
                Dim Message  As String  = String.Empty
                Dim success  As Boolean = Double.TryParse(RetValue, DoubleID)    
                
                ' Verify given ID.
                If ((Not success) OrElse Double.IsNaN(DoubleID)) Then
                    success = False
                    Message = sprintf(Rstyx.Utilities.Resources.Messages.GeoPointConstraints_IDNotNumeric, RetValue)
                Else
                    Dim MinDoubleID As Double  = MinIntegerID
                    Dim MaxDoubleID As Double  = MaxIntegerID
                    If (RetValue.Contains(".") OrElse ((DoubleID > 0.0) AndAlso (DoubleID < 1.0))) Then DoubleID *= PointNoFactor
                    DoubleID = Round(DoubleID, 0)
                    
                    If ((DoubleID < MinDoubleID) OrElse (DoubleID > MaxDoubleID)) Then
                        success = False
                        Message = sprintf(Rstyx.Utilities.Resources.Messages.GeoPointConstraints_IDOutOfIntRange, RetValue, MinIntegerID, MaxIntegerID)
                    End If
                End If
                If (Not success) Then Throw New InvalidIDException(Message)
                
                ' Format validated ID.
                RetValue = sprintf("%.0f", DoubleID)
                
                Return RetValue
            End Function
            
            ''' <summary> Returns a KV output of the point. </summary>
            Public Overrides Function ToString() As String
                
                Dim KvFmt As String = "%+7s %15.5f%15.5f%10.4f %12.4f  %-13s %-13s %-4s %4d %1s  %3s %5.0f %5.0f  %1s %+3s  %1s%1s  %-8s %7s"
                
                Return sprintf(KvFmt, Me.ID, Me.Y, Me.X, Me.Z, Me.TrackPos.Kilometer.Value, Me.Info.TrimToMaxLength(13), Me.HeightInfo.TrimToMaxLength(13),
                               Me.KindText.TrimToMaxLength(4), Me.TrackPos.TrackNo, Me.TrackPos.RailsCode.TrimToMaxLength(1), Me.HeightSys.TrimToMaxLength(3), Me.mp, Me.mh, 
                               Me.MarkHints.TrimToMaxLength(1), Me.MarkType.TrimToMaxLength(3), Me.sp.TrimToMaxLength(1), Me.sh.TrimToMaxLength(1),
                               Me.Job.TrimToMaxLength(8), Me.ObjectKey.TrimToMaxLength(7))
            End Function
            
        #End Region
        
    End Class

End Namespace

' for jEdit:  :collapseFolds=2::tabSize=4::indentSize=4:
