using System;

using Android.App;
using Android.Content.PM;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using Android.OS;

// We'll need the Agora package for downlinking the voice and video.
using DT.Xamarin.Agora;
using DT.Xamarin.Agora.Video;
using SystemDebug = System.Diagnostics.Debug;

namespace xf_native.Droid
{
    // Add the minimum event handlers for a one way connection.
    public class AgoraRtcHandler : IRtcEngineEventHandler
    {
        private MainActivity _context;

        public AgoraRtcHandler(MainActivity activity)
        {
            _context = activity;
        }

        public override void OnJoinChannelSuccess(string p0, int p1, int p2)
        {
            _context.OnJoinChannelSuccess(p0, p1, p2);
        }

        public override void OnFirstRemoteVideoDecoded(int p0, int p1, int p2, int p3)
        {
            _context.OnFirstRemoteVideoDecoded(p0, p1, p2, p3);
        }

        public override void OnUserJoined(int p0, int p1)
        {
            //SystemDebug.WriteLine("# JOIN Succeeded for: " + p0);
        }
    }

    [Activity(Label = "xf_native", Icon = "@mipmap/icon", Theme = "@style/MainTheme", MainLauncher = true, ConfigurationChanges = ConfigChanges.ScreenSize | ConfigChanges.Orientation)]
    public class MainActivity : global::Xamarin.Forms.Platform.Android.FormsAppCompatActivity
    {
        // Step #4
        protected RtcEngine AgoraEngine;
        protected AgoraRtcHandler AgoraHandler;
        // We'll use this layout to render the Agora video stream to.
        private View _layout;
        private uint _remoteId = 0;

        protected override void OnCreate(Bundle savedInstanceState)
        {
            TabLayoutResource = Resource.Layout.Tabbar;
            ToolbarResource = Resource.Layout.Toolbar;
            int ret = 0;

            base.OnCreate(savedInstanceState);
            SetContentView(layoutResID: Resource.Layout.RemoteStream1);
            _layout = FindViewById(Resource.Id.remote_stream1);

            Xamarin.Essentials.Platform.Init(this, savedInstanceState);
            global::Xamarin.Forms.Forms.Init(this, savedInstanceState);
            LoadApplication(new App());

            // Step #5
            AgoraHandler = new AgoraRtcHandler(this);
            AgoraEngine = RtcEngine.Create(BaseContext, "c6122ec557d54eb09783b44f90cbfc7b", AgoraHandler);
            AgoraEngine.EnableWebSdkInteroperability(true);

            AgoraEngine.EnableVideo();
            AgoraEngine.SetVideoProfile(Constants.VideoProfile360p, false);
            ret = AgoraEngine.JoinChannel(
                "006c6122ec557d54eb09783b44f90cbfc7bIACLw1riRkApB6HfNVEIKm7Ejz1U3Vd08e6z4rqsJNAsc1iNuxORY278FgACAAAAAAAAAAIAAQAAAAAAAwAAAAAA",
                "test2",
                string.Empty,
                198);
            SystemDebug.WriteLine("### JOIN CHANNEL RETURN: " + ret);

        }
        public override void OnRequestPermissionsResult(int requestCode, string[] permissions, [GeneratedEnum] Android.Content.PM.Permission[] grantResults)
        {
            Xamarin.Essentials.Platform.OnRequestPermissionsResult(requestCode, permissions, grantResults);

            base.OnRequestPermissionsResult(requestCode, permissions, grantResults);
        }

        // Step 3: Implement the event handlers for this specific activity.
        public void OnJoinChannelSuccess(string channel, int uid, int elapsed)
        {
            SystemDebug.WriteLine("# JOIN Succeeded for: " + uid);
        }

        public void OnFirstRemoteVideoDecoded(int uid, int width, int height, int elapsed)
        {
            RunOnUiThread(() =>
            {
                SetupRemoteVideo(uid);
            });
        }

        // Step 6: Implement the event handler for creating a surface and adding it to the main activity's view.
        private void SetupRemoteVideo(int uid)
        {
            _remoteId = (uint)uid;
            FrameLayout container = (FrameLayout)FindViewById(Resource.Id.remote_stream1); ;
            if (container.ChildCount >= 1)
            {
                return;
            }
            SurfaceView surfaceView = RtcEngine.CreateRendererView(BaseContext);
            container.AddView(surfaceView);

            AgoraEngine.SetupRemoteVideo(new VideoCanvas(surfaceView, VideoCanvas.RenderModeAdaptive, uid));
            surfaceView.Tag = uid; // for mark purpose
        }
    }
}