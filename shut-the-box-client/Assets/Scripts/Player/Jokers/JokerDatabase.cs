namespace Player.Jokers
{
    using Network;
    using UnityEngine;

    public interface IJokerDatabase
    {
        JokerModel this[Joker joker] { get; }
    }
    
    [CreateAssetMenu(fileName = "JokerDatabase", menuName = "JokerDatabase", order = 0)]
    public class JokerDatabase : ScriptableObject, IJokerDatabase
    {
        [SerializeField] 
        private JokerModel[] _jokers;

        public JokerModel this[Joker joker]
        {
            get
            {
                foreach (JokerModel model in _jokers)
                {
                    if (model.Type == joker) return model;
                }

                return default;
            }
        }
    }
}