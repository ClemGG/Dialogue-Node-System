using System.Collections.Generic;
using UnityEngine;
using static Project.Utilities.ValueTypes.Enums;

namespace Project.NodeSystem 
{

    [CreateAssetMenu(menuName = "Dialogue/New Dialogue Character", fileName = "New Dialogue Character")]
    [System.Serializable]
    public class DialogueCharacterSO : ScriptableObject
    {
        public List<string> CharacterNames = new List<string>(); //The names translated in all available languages
        public Color CharacterNameColor = Color.white;
        public AudioClip CharPrintClip;
        [SerializeField] private List<FaceAndMood> m_faces = new List<FaceAndMood>();



        private void OnValidate()
        {
            int length = LengthOf<LanguageType>();

            //Initializes the character's names when the ScriptableObject is reset or created for the 1st time.
            //Normally the character has always the same name, but it might be written in a different alphabet (latin, cyrillic, mandarin, etc.),
            //This allows us to change the caracters.
            if (CharacterNames == null ^ CharacterNames.Count < length)
            {
                for (int i = CharacterNames.Count; i < length; i++)
                {
                    CharacterNames.Add(string.Empty);
                }
            }


            length = LengthOf<CharacterMood>();

            //Initializes the sprites and enums when the ScriptableObject is reset or created for the 1st time.
            if (m_faces == null ^ m_faces.Count < length)
            {
                for (int i = m_faces.Count; i < length; i++)
                {
                    m_faces.Add(new FaceAndMood { Face = null, Mood = (CharacterMood)i });
                }
            }
        }



        public FaceAndMood GetFaceAndMood(int index)
        {
            return m_faces[index];
        }


        public Sprite GetFaceFromMood(CharacterMood mood)
        {
            return m_faces[(int)mood].Face;
        }



        
    }

    [System.Serializable]
    public class FaceAndMood
    {
        public Sprite Face;
        public CharacterMood Mood;
    }
}