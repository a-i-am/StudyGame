using System;
using UnityEngine;

namespace StudyGame.Editor.Director
{
    public enum LevelGeneratorType
    {
        BasicRoom,
        MultiStoryBuilding
    }

    [Serializable]
    public class ProBuilderLevelSettings
    {
        public LevelGeneratorType GeneratorType = LevelGeneratorType.BasicRoom;
        
        // --- Basic Room Settings ---
        public Vector3 RoomSize = new Vector3(10, 3, 10);
        public bool IsHollow = true; // true: 카메라가 내부에 있는 방(면 뒤집힘), false: 꽉 찬 플랫폼
        
        // --- Multi-Story Building Settings ---
        public int FloorCount = 2;
        public float FloorHeight = 4f;
        public float BuildingWidth = 15f;
        public float BuildingLength = 15f;
        public float FloorThickness = 0.5f;
        public bool IncludeStairs = true;
        public bool IncludePillars = true;
    }
}
