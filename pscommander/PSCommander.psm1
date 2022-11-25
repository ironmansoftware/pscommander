function New-CommanderToolbarIcon {
    <#
    .SYNOPSIS
    Creates a notification tray toolbar icon.
    
    .DESCRIPTION
    Creates a notification tray toolbar icon.
    
    .PARAMETER Text
    Text to display when the icon is hovered.
    
    .PARAMETER MenuItem
    Menu items to display when the icon is right clicked.
    
    .PARAMETER LoadMenuItems
    A script block to call to dynamically load menu items.
    
    .PARAMETER HideExit
    Hides the exit menu item.
    
    .PARAMETER HideConfig
    Hides the config menu item.

    .PARAMETER Icon
    Path to an icon file to display in the toolbar.
    
    .EXAMPLE
    New-CommanderToolbarIcon -MenuItem @(
        New-CommanderMenuItem -Text 'Notepad' -Action {
            Start-Process notepad
        } -MenuItem @(
            New-CommanderMenuItem -Text 'Subnotepad' -Action {
                Start-Process notepad
            }
        ) -LoadMenuItems {  
            New-CommanderMenuItem -Text 'Dynamic SubNotepad' -Action {
                Start-Process notepad
            }
        }
    ) -LoadMenuItems {
        New-CommanderMenuItem -Text 'Dynamic Notepad' -Action {
            Start-Process notepad
        }
    }

    Creates a tool bar icon with several menu items.


    #>
    [CmdletBinding()]
    param(
        [Parameter()]
        [string]$Text, 
        [Parameter()]
        [pscommander.MenuItem[]]$MenuItem,
        [Parameter()]
        [ScriptBlock]$LoadMenuItems,
        [Parameter()]
        [Switch]$HideExit,
        [Parameter()]
        [Switch]$HideConfig,
        [Parameter()]
        [string]$Icon
    )

    Process {
        $ToolbarIcon = [pscommander.ToolbarIcon]::new()
        $ToolbarIcon.Text = $Text 
        $ToolbarIcon.MenuItems = $MenuItem
        $ToolbarIcon.LoadItems = $LoadMenuItems
        $ToolbarIcon.HideExit = $HideExit
        $ToolbarIcon.HideConfig = $HideConfig
        $ToolbarIcon.Icon = $Icon
        $ToolbarIcon
    }
}

function New-CommanderMenuItem {
    <#
    .SYNOPSIS
    Creates a new menu item to use within a toolbar notification icon.
    
    .DESCRIPTION
    Creates a new menu item to use within a toolbar notification icon.
    
    .PARAMETER Text
    The text to display for this menu item.
    
    .PARAMETER Action
    A script block to invoke when the menu item is clicked.
    
    .PARAMETER MenuItem
    Child menu items to display.
    
    .PARAMETER LoadMenuItems
    Child menu items to load dynamically.

    .PARAMETER ArgumentList
    Arguments passed to the action.

    #>
    [CmdletBinding()]
    param(
        [Parameter()]
        [string]$Text,
        [Parameter()]
        [ScriptBlock]$Action, 
        [Parameter()]
        [pscommander.MenuItem[]]$MenuItem,
        [Parameter()]
        [ScriptBlock]$LoadMenuItems,
        [Parameter()]
        [object[]]$ArgumentList = @()
    )

    Process {
        $mi = [pscommander.MenuItem]::new()
        $mi.Text = $Text 
        $mi.Action = $Action
        $mi.Children = $MenuItem
        $mi.LoadChildren = $LoadMenuItems
        $mi.ArgumentList = $ArgumentList
        $mi
    }
}

function New-CommanderHotKey {
    <#
    .SYNOPSIS
    Creates a new global hot key binding.
    
    .DESCRIPTION
    Creates a new global hot key binding.
    
    .PARAMETER ModifierKey
    One or modifier keys to use for this hot key.
    
    .PARAMETER Key
    The main key to use for this hot key.
    
    .PARAMETER Action
    The action to invoke for this hot key.
    
    .EXAMPLE
    New-CommanderHotKey -Key 'T' -ModifierKey 'Ctrl' -Action { 
        Start-Process notepad
    }

    Starts notepad when Ctrl+T is pressed.
    #>
    [CmdletBinding()]
    param(
        [Parameter()]
        [pscommander.ModifierKeys]$ModifierKey,
        [Parameter(Mandatory)]
        [pscommander.Keys]$Key,
        [Parameter(Mandatory)]
        [ScriptBlock]$Action
    )

    $HotKey = [pscommander.HotKey]::new()
    $HotKey.Id = Get-Random
    $HotKey.ModifierKeys = $ModifierKey
    $HotKey.Keys = $Key
    $HotKey.Action = $Action

    $HotKey
}

function New-CommanderSchedule {
    <#
    .SYNOPSIS
    Creates a scheduled action based on a CRON expression.
    
    .DESCRIPTION
    Creates a scheduled action based on a CRON expression.
    
    .PARAMETER Action
    The action to execute on the schedule. 
    
    .PARAMETER CronExpression
    The CRON expression that defines when to run the action.
    
    .EXAMPLE
    New-CommanderSchedule -CronExpression "* * * * *" -Action {
        Start-Process Notepad
    }

    Starts notepad every minute.
    #>
    [CmdletBinding()]
    param(
        [Parameter()]
        [ScriptBlock]$Action,
        [Parameter()]
        [string]$CronExpression
    )

    $Schedule = [pscommander.Schedule]::new()
    $Schedule.Action = $Action 
    $Schedule.Cron = $CronExpression
    
    $Schedule
}

function New-CommanderContextMenu {
    <#
    .SYNOPSIS
    Creates a context menu item that executes PowerShell.
    
    .DESCRIPTION
    Creates a context menu item that executes PowerShell.
    
    .PARAMETER Action
    The script block action to execute. $Args[0] will include the path the was right clicked.
    
    .PARAMETER Text
    The text to display.
    
    .PARAMETER Location
    The location to display this context menu item. File will display this action when right clicking on the associated file extension. FolderLeftPane will display when right clicking on a folder in the left pane of the explorer window. FolderRightPane will display when right clicking on the folder in the right pane of the explorer window or the desktop.
    
    .PARAMETER Extension
    The extension to associate this context menu item to. This requires that Location is set to File. 
    
    .PARAMETER DisplayOnShiftClick
    Displays this option only when shift is help down during the right click.
    
    .PARAMETER Position
    The location to position this context menu item. You can select Top, Bottom and None. None is the default. 
    
    .PARAMETER Icon
    An icon to display for this context menu item.
    
    .PARAMETER IconIndex
    The index within the icon file to use. 
    
    .EXAMPLE
    New-CommanderContextMenu -Text 'Click me' -Action {
        Start-Process code -ArgumentList $args[0]
    }

    Starts VS Code and opens the file that was right clicked. 
    #>
    [CmdletBinding()]
    param(
        [Parameter(Mandatory)]
        [ScriptBlock]$Action, 
        [Parameter(Mandatory)]
        [string]$Text, 
        [Parameter()]
        [ValidateSet("FolderLeftPanel", "FolderRightPanel", "File")]
        [string]$Location = "File", 
        [Parameter()]
        [string]$Extension = "*",
        [Parameter()]
        [Switch]$DisplayOnShiftClick,
        [Parameter()]
        [ValidateSet("Top", "Bottom", "None")]
        [string]$Position = 'None',
        [Parameter()]
        [string]$Icon,
        [Parameter()]
        [int]$IconIndex
    )

    $ContextMenu = [pscommander.ExplorerContextMenu]::new()
    $ContextMenu.Id = Get-Random
    $ContextMenu.Action = $Action 
    $ContextMenu.Text = $Text 
    $ContextMenu.Location = $Location 
    $ContextMenu.Extension = $Extension 
    $ContextMenu.Extended = $DisplayOnShiftClick
    $ContextMenu.Position = $Position
    $ContextMenu.Icon = $Icon 
    $ContextMenu.IconIndex = $IconIndex
    $ContextMenu 
}

function New-CommanderFileAssociation {
    <#
    .SYNOPSIS
    Creates a file association that will invoke the action when it's opened.
    
    .DESCRIPTION
    Creates a file association that will invoke the action when it's opened.
    
    .PARAMETER Extension
    The extension to associate with the action. 
    
    .PARAMETER Action
    The action to execute when the file type is opened. $Args[0] will be the full file name of the file opened. 
    
    .EXAMPLE
    New-CommanderFileAssociation -Extension ".ps2" -Action {
        Start-Process code -ArgumentList $Args[0]
    }

    Starts VS Code and opens the opened PS2 file. 
    #>
    [CmdletBinding()]
    param(
        [Parameter(Mandatory)]
        [string]$Extension,
        [Parameter(Mandatory)]
        [scriptblock]$Action
    )

    if (-not $Extension.StartsWith('.')) {
        throw "Extension needs to start with '.'"
    }

    $FileAssociation = [pscommander.FileAssociation]::new()
    $FileAssociation.Id = Get-Random
    $FileAssociation.Extension = $Extension
    $FileAssociation.Action = $Action
    $FileAssociation
}

function New-CommanderCustomProtocol {
    <#
    .SYNOPSIS
    Creates a custom protocol handler. 
    
    .DESCRIPTION
    Creates a custom protocol handler. 
    
    .PARAMETER Protocol
    The protcol scheme to use. 
    
    .PARAMETER Action
    The action to execute when the file type is opened. $Args[0] will be the full file name of the file opened. 
    
    .EXAMPLE
    New-CommanderCustomProtocol -Protocol "Commander" -Action {
        Start-Process code -ArgumentList $Args[0]
    }

    Starts code when the Commander protocol is used. Commander://test.txt 
    #>
    [CmdletBinding()]
    param(
        [Parameter(Mandatory)]
        [string]$Protocol,
        [Parameter(Mandatory)]
        [scriptblock]$Action
    )

    $CustomProtocol = [pscommander.CustomProtocol]::new()
    $CustomProtocol.Protocol = $Protocol
    $CustomProtocol.Action = $Action
    $CustomProtocol
}

function New-CommanderShortcut {
    <#
    .SYNOPSIS
    Creates a new desktop shortcut that will run the action.
    
    .DESCRIPTION
    Creates a new desktop shortcut that will run the action.
    
    .PARAMETER Text
    The text to display on the desktop.
    
    .PARAMETER Description
    The description shown when hovering the shortcut. 
    
    .PARAMETER Icon
    The icon to display.
    
    .PARAMETER Action
    The action to execute when the shortcut is clicked. 
    
    .EXAMPLE
    New-CommanderShortcut -Text 'Click Me' -Description 'Nice' -Action {
        Start-Process notepad
    }

    Creates a shortcut that will start notepad when clicked.
    #>
    [CmdletBinding()]
    param(
        [Parameter(Mandatory)]
        [string]$Text,
        [Parameter()]
        [string]$Description,
        [Parameter()]
        [string]$Icon,
        [Parameter(Mandatory)]
        [ScriptBlock]$Action
    )

    $Shortcut = [pscommander.Shortcut]::new()
    $Shortcut.Id = Get-Random
    $Shortcut.Text = $Text
    $Shortcut.Description = $Description
    $Shortcut.Icon = $Icon
    $Shortcut.Action = $Action
    $Shortcut
}

function Start-Commander {
    <#
    .SYNOPSIS
    Starts PSCommander. 
    
    .DESCRIPTION
    Starts PSCommander
    
    .EXAMPLE
    Start-Commander

    Starts PSCommander
    #>

    param($ConfigPath)

    if (-not $ConfigPath) {
        $Documents = [System.Environment]::GetFolderPath('MyDocuments')
        $ConfigPath = [IO.Path]::Combine($Documents, 'PSCommander', 'config.ps1')
    }

    if (-not (Test-Path $ConfigPath)) {
        Write-Warning "Configuration file for PSCommander not found. Creating config file..."
        New-Item -Path (Join-Path $Documents 'PSCommander') -ItemType Directory -Force -ErrorAction SilentlyContinue | Out-Null
        "New-CommanderToolbarIcon -MenuItem @( 
    New-CommanderMenuItem -Text 'Documentation' -Action { 
        Start-Process 'https://docs.poshtools.com/powershell-pro-tools-documentation/pscommander' 
    } 
)" | Out-File $ConfigPath 
        Start-Process -FilePath "$PSScriptRoot\psscriptpad.exe" -ArgumentList @("-c `"$ConfigPath`"")
    }

    Start-Process (Join-Path $PSScriptRoot "PSCommander.exe") -ArgumentList "--configFilePath '$ConfigPath'"
}

function Install-Commander {
    <#
    .SYNOPSIS
    Sets commander to run on logon.

    .DESCRIPTION
    Sets commander to run on logon.

    .EXAMPLE
    Install-Commander

    Sets commander to run on logon.
    #>
    New-ItemProperty -Path "HKCU:\SOFTWARE\Microsoft\Windows\CurrentVersion\Run" -Name 'PSCommander' -Value (Join-Path $PSScriptRoot "pscommander.exe") -Force | Out-Null 
}

function Uninstall-Commander {
    <#
    .SYNOPSIS
    Stops commander from running on logon.
    
    .DESCRIPTION
    Stops commander from running on logon.
    
    .EXAMPLE
    Uninstall-Commander

    Stops commander from running on logon.
    #>
    Remove-ItemProperty -Path "HKCU:\SOFTWARE\Microsoft\Windows\CurrentVersion\Run" -Name 'PSCommander'
}

function Register-CommanderEvent {
    <#
    .SYNOPSIS
    Registers a handler to invoke when an event takes place.
    
    .DESCRIPTION
    Registers a handler to invoke when an event takes place.
    
    .PARAMETER OnCommander
    Specifies event handlers for events within commander. 

    .PARAMETER OnWindows
    Specifies event handlers for events within Windows.

    .PARAMETER WmiEventType
    Specifies the WMI event type to query when using -OnWindows WmiEvent

    .PARAMETER WmiEventFilter
    Specifies the WMI event filter to query when using -OnWindows WmiEvent
    
    .PARAMETER Action
    The action to invoke when an event takes place.
    
    .EXAMPLE
    Register-CommanderEvent -OnCommander Start -Action {
        Start-Process notepad
    }

    Starts notepad when commander starts.
    #>
    [CmdletBinding()]
    param(
        [Parameter(Mandatory, ParameterSetName = "Commander")]
        [ValidateSet('start', 'stop', 'error')]
        [string]$OnCommander,
        [Parameter(Mandatory, ParameterSetName = "Windows")]
        [ValidateSet('ProcessStarted', 'WmiEvent')]
        [string]$OnWindows,
        [Parameter(ParameterSetName = "Windows")]
        [string]$WmiEventType,
        [Parameter(ParameterSetName = "Windows")]
        [string]$WmiEventFilter,
        [Parameter(Mandatory)]
        [ScriptBlock]$Action
    )

    $CommanderEvent = [pscommander.CommanderEvent]::new()
    $CommanderEvent.Id = Get-Random
    $CommanderEvent.Category = $PSCmdlet.ParameterSetName

    if ($OnCommander) {
        $CommanderEvent.Event = $OnCommander
    }
    
    if ($OnWindows) {
        $CommanderEvent.Event = $OnWindows
        if ($OnWindows -eq 'ProcessStarted') {
            $CommanderEvent.Properties['WmiEventType'] = "__InstanceCreationEvent"
            $CommanderEvent.Properties['WmiEventFilter'] = 'TargetInstance isa "Win32_Process"'
        }

        if ($OnWindows -eq 'WmiEvent') {
            $CommanderEvent.Properties['WmiEventType'] = $WmiEventType
            $CommanderEvent.Properties['WmiEventFilter'] = $WmiEventFilter
        }
    }

    $CommanderEvent.Action = $Action
    $CommanderEvent
}

function Stop-Commander {
    <#
    .SYNOPSIS
    Stops commander
    
    .DESCRIPTION
    Stops commander
    
    .EXAMPLE
    Stop-Commander
    #>
    Get-Process PSCommander | Stop-Process
}

function Set-CommanderSetting {
    <#
    .SYNOPSIS
    Set commander settings.
    
    .DESCRIPTION
    Set commander settings.
    
    .PARAMETER DisableUpdateCheck
    Does not check for updates when starting commander. Find-Module is used to check for updates. 
    
    .EXAMPLE
    Set-CommanderSetting -DisableUpdateCheck
    #>
    param(
        [Parameter()]
        [Switch]$DisableUpdateCheck
    )

    $Settings = [pscommander.Settings]::new()
    $Settings.DisableUpdateCheck = $DisableUpdateCheck
    $Settings
}

function New-CommanderDesktop {
    param(
        [Parameter(Mandatory)]
        [pscommander.DesktopWidget[]]$Widget
    )

    $Desktop = [pscommander.Desktop]::new();
    $Desktop.Widgets = $Widget 
    $Desktop
}

function Set-CommanderDesktop {
    <#
    .SYNOPSIS
    Sets the commander desktop widgets.
    
    .DESCRIPTION
    Sets the commander desktop widgets. This cmdlet must be run within PSCommander.
    
    .PARAMETER Widget
    Widgets to display on the desktop.
    #>
    param(
        [Parameter(Mandatory)]
        [pscommander.DesktopWidget[]]$Widget
    )

    if ($DesktopService -eq $null) {
        throw 'This cmdlet only works when running with PSCommander'
    }

    $Desktop = [pscommander.Desktop]::new();
    $Desktop.Widgets = $Widget 
    $Desktop
    
    $DesktopService.SetDesktop($Desktop)
}

function Clear-CommanderDesktop {
    <#
    .SYNOPSIS
    Clears the commander desktop.
    
    .DESCRIPTION
    Clears the commander desktop. This cmdlet must be run within PSCommander.
    
    .EXAMPLE
    Clear-CommanderDesktop
    #>
    if ($DesktopService -eq $null) {
        throw 'This cmdlet only works when running with PSCommander'
    }

    $DesktopService.ClearDesktop()
}

function New-CommanderDesktopWidget {
    <#
    .SYNOPSIS
    Creates a desktop widget.
    
    .DESCRIPTION
    Creates a desktop widget. Desktop widgets display data. They appear on top of the wall paper but under icons. They are not interactive. 
    
    .PARAMETER Top
    The top location of the widget.
    
    .PARAMETER Left
    The left location of the widget.
    
    .PARAMETER Height
    The height of the widget. 
    
    .PARAMETER Width
    The width of the widget. 
    
    .PARAMETER MeasurementHistory
    The number of measurements to keep in the graph.
    
    .PARAMETER MeasurementFrequency
    How frequently to record a new measurements in seconds. 
    
    .PARAMETER LoadMeasurement
    A script block that records a measurement. Expected to return a number.
    
    .PARAMETER MeasurementTitle
    The title of the measurement.
    
    .PARAMETER MeasurementSubtitle
    The subtitle of the measurement. 
    
    .PARAMETER MeasurementDescription
    The description of the measurement. 
    
    .PARAMETER MeasurementUnit
    The unit to display for the measurement. 
    
    .PARAMETER MeasurementTheme
    The theme to use for the measurement. 
    
    .PARAMETER LoadWidget
    Loads a custom WPF widget. You will need to return a Window. The window will not be interactive.
    
    .PARAMETER Url
    Displays the webpage specified by the URL.
    
    .PARAMETER Image
    An image to display. 
    
    .PARAMETER Text
    Text to display.
    
    .PARAMETER Font
    A font to use with the text. 
    
    .PARAMETER BackgroundColor
    A background color for the text. If absent, the wall paper will be used. 
    
    .PARAMETER FontColor
    The font color to use for the text. 
    
    .PARAMETER FontSize
    The font size to use for the text. 

    .PARAMETER DataSource
    The data source to load data from. 
    
    .EXAMPLE
    New-CommanderDesktopWidget -Text 'Hello, world!' -Height 200 -Width 1000 -FontSize 100 -Top 500 -Left 500

    Displays text on the desktop.

    .EXAMPLE
    New-CommanderDesktopWidget -Image 'C:\src\blog\content\images\news.png' -Height 200 -Width 200 -Top 200 

    Displays an image on the desktop. 

    .EXAMPLE
    New-CommanderDesktopWidget -Url 'https://www.google.com' -Height 500 -Width 500 -Top 400

    Displays a webpage on the desktop.

    .EXAMPLE
    New-CommanderDesktopWidget -LoadWidget {
       [xml]$Form = "<Window xmlns=`"http://schemas.microsoft.com/winfx/2006/xaml/presentation`"><Grid><Label Content=`"Hello, World`" Height=`"30`" Width=`"110`"/></Grid></Window>"
		$XMLReader = (New-Object System.Xml.XmlNodeReader $Form)
		[Windows.Markup.XamlReader]::Load($XMLReader)
   } -Height 200 -Width 200 -Top 200 -Left 200

   Displays a custom WPF window on the desktop.

   .EXAMPLE 
   New-CommanderDesktopWidget -LoadMeasurement {Get-Random} -MeasurementTitle 'Test' -MeasurementSubtitle 'Tester' -MeasurementUnit '%' -Height 300 -Width 500 -Left 600 -Top 200 -MeasurementFrequency 1 -MeasurementDescription "Nice" -MeasurementTheme 'DarkBlue'

   Displays a measurement graph on the desktop.
    
    .NOTES
    General notes
    #>
    [CmdletBinding(DefaultParameterSetName = "Custom")]
    param(
        [Parameter()]
        [int]$Top,
        [Parameter()]
        [int]$Left,
        [Parameter()]
        [int]$Height = 12,
        [Parameter()]
        [int]$Width = 100,
        [Parameter()]
        [switch]$DisableTransparency,
        [Parameter(ParameterSetName = "Measurement")]
        [int]$MeasurementHistory = 100,
        [Parameter(ParameterSetName = "Measurement")]
        [int]$MeasurementFrequency = 30,
        [Parameter(Mandatory, ParameterSetName = "Measurement")]
        [ScriptBlock]$LoadMeasurement,
        [Parameter(Mandatory, ParameterSetName = "Measurement")]
        [string]$MeasurementTitle,
        [Parameter(Mandatory, ParameterSetName = "Measurement")]
        [string]$MeasurementSubtitle,
        [Parameter(ParameterSetName = "Measurement")]
        [string]$MeasurementDescription,
        [Parameter(Mandatory, ParameterSetName = "Measurement")]
        [string]$MeasurementUnit,
        [Parameter(ParameterSetName = "Measurement")]
        [ValidateSet("LightRed", 'LightGreen', 'LightBlue', "DarkRed", 'DarkGreen', 'DarkBlue')]
        [string]$MeasurementTheme = "LightRed",
        [Parameter(Mandatory, ParameterSetName = "Custom")]
        [Parameter(Mandatory, ParameterSetName = "DataSource")]
        [ScriptBlock]$LoadWidget,
        [Parameter(Mandatory, ParameterSetName = "Url")]
        [string]$Url,
        [Parameter(Mandatory, ParameterSetName = "Image")]
        [string]$Image,
        [Parameter(Mandatory, ParameterSetName = "Text")]
        [string]$Text,
        [Parameter(ParameterSetName = "Text")]
        [string]$Font,
        [Parameter(ParameterSetName = "Text")]
        [string]$BackgroundColor,
        [Parameter(ParameterSetName = "Text")]
        [string]$FontColor = '#fff',
        [Parameter(ParameterSetName = "Text")]
        [int]$FontSize = 12,
        [Parameter(ParameterSetName = "DataSource")]
        [string]$DataSource
    )

    $Widget = $null 
    if ($PSCmdlet.ParameterSetName -eq 'Text') {
        $Widget = [pscommander.TextDesktopWidget]::new()
        $Widget.Text = $Text
        $Widget.Font = $Font 
        $Widget.BackgroundColor = $BackgroundColor 
        $Widget.FontColor = $FontColor 
        $Widget.FontSize = $FontSize
    }

    if ($PScmdlet.ParameterSetName -eq 'Image') {
        $Widget = [pscommander.ImageDesktopWidget]::new() 
        $Widget.Image = $Image
    }

    if ($PSCmdlet.ParameterSetName -eq 'Url') {
        $Widget = [pscommander.WebpageDesktopWidget]::new()
        $Widget.Url = $Url
    }

    if ($PSCmdlet.ParameterSetName -eq 'Custom') {
        $Widget = [pscommander.CustomDesktopWidget]::new()
        $Widget.LoadWidget = $LoadWidget
    }

    if ($PSCmdlet.ParameterSetName -eq 'DataSource') {
        $Widget = [pscommander.DataDesktopWidget]::new()
        $Widget.LoadWidget = $LoadWidget
        $Widget.DataSource = $DataSource
    }

    if ($PSCmdlet.ParameterSetName -eq 'Measurement') {
        $Widget = [pscommander.MeasurementDesktopWidget]::new()
        $Widget.LoadMeasurement = $LoadMeasurement
        $Widget.Title = $MeasurementTitle
        $Widget.Subtitle = $MeasurementSubtitle
        $Widget.Unit = $MeasurementUnit
        $Widget.Frequency = $MeasurementFrequency
        $Widget.History = $MeasurementHistory
        $Widget.Description = $MeasurementDescription
        $Widget.Theme = $MeasurementTheme
    }

    $Widget.Top = $Top
    $Widget.Left = $Left
    $Widget.Height = $Height
    $Widget.Width = $Width
    $Widget.Transparent = -not $DisableTransparency.IsPresent
    $Widget
}

function Register-CommanderDataSource {
    <#
    .SYNOPSIS
    Registers a custom data source script block to run on an interval.
    
    .DESCRIPTION
    Registers a custom data source script block to run on an interval. Data sources can be used with desktop widgets. 
    
    .PARAMETER Name
    The name of the data source. 
    
    .PARAMETER LoadData
    The data to load. 
    
    .PARAMETER RefreshInterval
    The refresh interval in seconds. 
    
    .PARAMETER HistoryLimit
    The amount of history to retain. 
    
    .EXAMPLE
    Register-CommanderDataSource -Name 'ComputerInfo' -LoadData {
        $Stats = Get-NetAdapterStatistics
        $NetworkDown = 0
        $Stats.ReceivedBytes | Foreach-Object { $NetworkDown += $_ } 
        
        $NetworkUp = 0
        $Stats.SentBytes | Foreach-Object { $NetworkUp += $_ } 
            
        @{
            CPU = Get-CimInstance Win32_Processor | Measure-Object -Property LoadPercentage -Average | Select-Object -Expand Average
            Memory = (Get-Counter '\Memory\Available MBytes').CounterSamples.CookedValue
            NetworkUp = $NetworkUp / 1KB
            NetworkDown = $NetworkDown / 1KB
        }
    } -RefreshInterval 5

    Gathers computer information and stores it as a data source.
    #>
    param(
        [Parameter(Mandatory)]
        [string]$Name, 
        [Parameter(Mandatory)]
        [ScriptBlock]$LoadData, 
        [Parameter()]
        [int]$RefreshInterval = 60,
        [Parameter()]
        [int]$HistoryLimit = 10 ,
        [Parameter()]
        [object[]]$ArgumentList = @()
    )

    $DataSource = [pscommander.DataSource]::new()
    $DataSource.Name = $Name 
    $DataSource.LoadData = $LoadData 
    $DataSource.RefreshInterval = $RefreshInterval
    $DataSource.HistoryLimit = $HistoryLimit
    $DataSource.ArgumentList = $ArgumentList
    $DataSource
}

function New-CommanderBlink {
    param(
        [Parameter(Mandatory)]
        [string]$Path
    )

    $Blink = [pscommander.Blink]::new()
    $Blink.Path = $Path
    $Blink
}