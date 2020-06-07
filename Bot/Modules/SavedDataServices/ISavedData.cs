namespace SmashBotUltimate.Bot.Modules.SavedDataServices {
    public interface ISavedData<Key, Value> {
        bool HasData (Key key);
        void SaveData (Key key, Value value);
        void RemoveData (Key key);
        bool TryGetData (Key key, out Value value);
    }
}