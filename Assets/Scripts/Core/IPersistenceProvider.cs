namespace StudyGame.Core
{
    public interface IPersistenceProvider
    {
        void Save(GameSaveData data);
        GameSaveData Load();
        void Reset();
    }
}
