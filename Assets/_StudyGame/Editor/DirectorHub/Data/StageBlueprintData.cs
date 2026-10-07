using System;
using System.Collections.Generic;
using UnityEngine;

namespace StudyGame.Editor.Director
{
    public enum CompositionTrackType
    {
        Environment,
        Scenario,
        Camera,
        Factory,
        Data,
        TestRunner
    }

    [Serializable]
    public class CompositionClip
    {
        public string ClipId = Guid.NewGuid().ToString();
        public string Label = "New Clip";
        public float TimeStart = 0f;
        public float Duration = 3f;
        public Color ClipColor = new Color(0.3f, 0.6f, 0.9f);

        public string FloorplanJsonPath;
        public string EpisodeGraphPath;
        public string CameraPresetKey;
        public string FactoryMenuCommand;
        public string DataAssetPath;
        public string SceneObjectName;
        public string ScenePath;

        public string SerializedPayload;
    }

    [Serializable]
    public class CompositionTrack
    {
        public string TrackId = Guid.NewGuid().ToString();
        public string TrackName = "Track";
        public CompositionTrackType TrackType;
        public Color TrackColor = Color.gray;
        public List<CompositionClip> Clips = new List<CompositionClip>();
    }

    [Serializable]
    public class StageBlueprintData
    {
        public string BlueprintName = "Untitled Stage";
        public List<CompositionTrack> Tracks = new List<CompositionTrack>();

        public static StageBlueprintData CreateDefault()
        {
            var bp = new StageBlueprintData();
            bp.Tracks.Add(new CompositionTrack
            {
                TrackName = "🏗️ 환경",
                TrackType = CompositionTrackType.Environment,
                TrackColor = new Color(0.2f, 0.7f, 0.3f)
            });
            bp.Tracks.Add(new CompositionTrack
            {
                TrackName = "🕸️ 시나리오",
                TrackType = CompositionTrackType.Scenario,
                TrackColor = new Color(0.3f, 0.5f, 0.9f)
            });
            bp.Tracks.Add(new CompositionTrack
            {
                TrackName = "⚔️ 카메라/연출",
                TrackType = CompositionTrackType.Camera,
                TrackColor = new Color(0.9f, 0.4f, 0.3f)
            });
            bp.Tracks.Add(new CompositionTrack
            {
                TrackName = "🏭 공장",
                TrackType = CompositionTrackType.Factory,
                TrackColor = new Color(0.8f, 0.6f, 0.2f)
            });
            bp.Tracks.Add(new CompositionTrack
            {
                TrackName = "📄 데이터",
                TrackType = CompositionTrackType.Data,
                TrackColor = new Color(0.6f, 0.4f, 0.8f)
            });
            bp.Tracks.Add(new CompositionTrack
            {
                TrackName = "▶️ 러너",
                TrackType = CompositionTrackType.TestRunner,
                TrackColor = new Color(0.4f, 0.8f, 0.8f)
            });
            return bp;
        }
    }
}
