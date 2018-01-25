Imports System.Windows
Imports System.IO
Imports System.Windows.Media
Imports System.Windows.Threading
Imports System.Windows.Input
Imports GameAdministratorCenter.Contracts

Public Class MediaPlayer
    Public Delegate Sub afterCloseFunction()
    Public afterCloseFunctionDelegate As afterCloseFunction = Nothing
    Private tempFileName As String = ""
    Private isPlaying As Boolean = False
    Public auto_play As Boolean = False

    Public Sub New(inputString As String, Optional inputStringType As InputStringType = InputStringType.mediaString)

        ' This call is required by the designer.
        InitializeComponent()

        If inputStringType = InputStringType.mediaString Then
            Try
                loadmediaString(inputString)
            Catch ex As Exception
                layoutGroupControl.Header = "Error Reading media String"
            End Try
        Else
            Try
                mediaPlayer.Source = New Uri(inputString)
                mediaPlayer.Pause()
            Catch ex As Exception
                layoutGroupControl.Header = "Error Reading media File"
            End Try
        End If
    End Sub

    Private Sub loadmediaString(inputString As String)
        tempFileName = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString() + ".mp4")
        Dim mediaByteArray As Byte() = Convert.FromBase64String(inputString)
        File.WriteAllBytes(tempFileName, mediaByteArray)
        mediaPlayer.Source = New Uri(tempFileName)
        mediaPlayer.Pause()
    End Sub

    Public Sub New(managerID As Integer)

        ' This call is required by the designer.
        InitializeComponent()

        ' Add any initialization after the InitializeComponent() call.
        Dim mediaManager As CONTENT_MANAGER = (From query In getDatabaseEntity().CONTENT_MANAGER Where query.ID = managerID).FirstOrDefault
        If mediaManager IsNot Nothing Then
            If mediaManager.CONTENT_MANAGER_TYPE = ContentManagerType.Audio Then
                Dim associatedMediaStorage As AUDIO_STORAGE = mediaManager.AUDIO_STORAGE
                If associatedMediaStorage IsNot Nothing AndAlso associatedMediaStorage.AUDIO_STRING IsNot Nothing Then
                    Try
                        loadmediaString(associatedMediaStorage.AUDIO_STRING)
                    Catch ex As Exception
                        layoutGroupControl.Header = "Error Reading media String from Content Manager"
                    End Try
                End If
            ElseIf mediaManager.CONTENT_MANAGER_TYPE = ContentManagerType.Video Then
                Dim associatedMediaStorage As VIDEO_STORAGE = mediaManager.VIDEO_STORAGE
                If associatedMediaStorage IsNot Nothing AndAlso associatedMediaStorage.VIDEO_STRING IsNot Nothing Then
                    Try
                        loadmediaString(associatedMediaStorage.VIDEO_STRING)
                    Catch ex As Exception
                        layoutGroupControl.Header = "Error Reading media String from Content Manager"
                    End Try
                End If
            End If
        End If
    End Sub

    Private Sub playButton_Click(sender As Object, e As RoutedEventArgs) Handles playButton.Click
        If Not isPlaying Then
            If mediaPlayer.HasVideo Then
                mediaPlayer.ScrubbingEnabled = True
            End If

            mediaPlayer.Play()
            isPlaying = True
            Dim brsh = TryCast(playButton.Background, ImageBrush)
            If brsh IsNot Nothing Then
                brsh.ImageSource = ImageProcessing.ImageSourceToBitmapImage(My.Resources.Pause)
            End If
        Else
            mediaPlayer.Pause()
            isPlaying = False
            Dim brsh = TryCast(playButton.Background, ImageBrush)
            If brsh IsNot Nothing Then
                brsh.ImageSource = ImageProcessing.ImageSourceToBitmapImage(My.Resources.Play)
            End If
        End If
    End Sub

    Private Sub stopButton_Click(sender As Object, e As RoutedEventArgs) Handles stopButton.Click
        If isPlaying Then
            mediaPlayer.Stop()
            isPlaying = False
            Dim brsh = TryCast(playButton.Background, ImageBrush)
            If brsh IsNot Nothing Then
                brsh.ImageSource = ImageProcessing.ImageSourceToBitmapImage(My.Resources.Play)
            End If
        End If
        mediaPlayer.Position = TimeSpan.FromSeconds(0)
        progressBar.Value = 0
    End Sub


    Private Sub closeButton_Click(sender As Object, e As RoutedEventArgs) Handles closeButton.Click
        If afterCloseFunctionDelegate IsNot Nothing Then
            afterCloseFunctionDelegate()
        End If
    End Sub

    Private Sub mediaPlayer_Unloaded(sender As Object, e As RoutedEventArgs) Handles Me.Unloaded
        Try
            mediaPlayer.Stop()
            File.Delete(tempFileName)
        Catch ex As Exception

        End Try
    End Sub

    Private Sub mediaPlayer_Loaded(sender As Object, e As RoutedEventArgs) Handles Me.Loaded
        Dim timer As New DispatcherTimer()
        timer.Interval = TimeSpan.FromMilliseconds(100)
        AddHandler timer.Tick, AddressOf timerTickEvent
        timer.Start()
        volumeSlider.Value = mediaPlayer.Volume
    End Sub

    Private Sub timerTickEvent(sender As Object, e As EventArgs)
        If isPlaying AndAlso (mediaPlayer.Source IsNot Nothing) AndAlso (mediaPlayer.NaturalDuration.HasTimeSpan) And Not progressBar.IsMouseCaptured Then
            progressBar.Minimum = 0
            progressBar.Maximum = mediaPlayer.NaturalDuration.TimeSpan.TotalSeconds
            progressBar.Value = mediaPlayer.Position.TotalSeconds
        End If
    End Sub

    Private Sub progressBar_ValueChanged(sender As Object, e As RoutedPropertyChangedEventArgs(Of Double)) Handles progressBar.ValueChanged
        mediaPlayer.Position = TimeSpan.FromSeconds(progressBar.Value)
        playTimeTextBlock.Text = mediaPlayer.Position.ToString("hh\:mm")
    End Sub

    Private Sub progressBar_MouseWheel(sender As Object, e As MouseWheelEventArgs) Handles progressBar.MouseWheel
        If e.Delta > 0 Then
            mediaPlayer.Position += TimeSpan.FromSeconds(5)
        Else
            mediaPlayer.Position -= TimeSpan.FromSeconds(5)
        End If
    End Sub

    Private Sub speedUpButon_Click(sender As Object, e As RoutedEventArgs) Handles speedUpButon.Click
        If mediaPlayer.SpeedRatio < 8 Then
            mediaPlayer.SpeedRatio *= 2
            speedTextBlock.Text = mediaPlayer.SpeedRatio.ToString("N1")
        End If
    End Sub

    Private Sub slowDownButton_Click(sender As Object, e As RoutedEventArgs) Handles slowDownButton.Click
        If mediaPlayer.SpeedRatio > 0.25 Then
            mediaPlayer.SpeedRatio /= 2
            speedTextBlock.Text = mediaPlayer.SpeedRatio.ToString("N1")
        End If
    End Sub

    Private Sub volumeSlider_ValueChanged(sender As Object, e As RoutedPropertyChangedEventArgs(Of Double)) Handles volumeSlider.ValueChanged
        mediaPlayer.Volume = volumeSlider.Value
    End Sub

    Private Sub progressBar_PreviewMouseLeftButtonDown(sender As Object, e As MouseButtonEventArgs) Handles progressBar.PreviewMouseLeftButtonDown
        mediaPlayer.Pause()
        isPlaying = False
    End Sub

    Private Sub progressBar_PreviewMouseLeftButtonUp(sender As Object, e As MouseButtonEventArgs) Handles progressBar.PreviewMouseLeftButtonUp
        mediaPlayer.Position = TimeSpan.FromSeconds(progressBar.Value)
        playTimeTextBlock.Text = mediaPlayer.Position.ToString("hh\:mm")
        mediaPlayer.Play()
        isPlaying = True
    End Sub

    Public Enum InputStringType
        fileName = 1
        mediaString = 2
    End Enum

    Private Sub mediaPlayer_MediaEnded(sender As Object, e As RoutedEventArgs)
        If repeat_check_edit.IsChecked Then
            progressBar.Value = 0
            mediaPlayer.Position = TimeSpan.FromSeconds(progressBar.Value)
            playTimeTextBlock.Text = mediaPlayer.Position.ToString("hh\:mm")
            mediaPlayer.Play()
            isPlaying = True
            Dim brsh = TryCast(playButton.Background, ImageBrush)
            If brsh IsNot Nothing Then
                brsh.ImageSource = ImageProcessing.ImageSourceToBitmapImage(My.Resources.Pause)
            End If
        Else
            mediaPlayer.Stop()
            isPlaying = False
            Dim brsh = TryCast(playButton.Background, ImageBrush)
            If brsh IsNot Nothing Then
                brsh.ImageSource = ImageProcessing.ImageSourceToBitmapImage(My.Resources.Refresh)
            End If
            mediaPlayer.Position = TimeSpan.FromSeconds(0)
            progressBar.Value = 0

        End If
    End Sub

    Private Sub UserControl_Loaded(sender As Object, e As RoutedEventArgs)
        If afterCloseFunctionDelegate Is Nothing Then
            closeButton.Visibility = Visibility.Collapsed
        Else
            closeButton.Visibility = Visibility.Visible
        End If
        If auto_play Then
            mediaPlayer.Play()
        End If
    End Sub
End Class
