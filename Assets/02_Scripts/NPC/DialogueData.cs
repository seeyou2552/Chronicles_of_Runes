[System.Serializable]
public class DialogueData
{
    public CharacterInfo Left;
    public CharacterInfo Right;
    public DialogueLine[] Dialogues;
}

[System.Serializable]
public class CharacterInfo
{
    public string name;
    public string portrait;
    public string profile;
}

[System.Serializable]
public class DialogueLine
{
    public int seq;
    public string speaker; // "Left" or "Right"
    public string text;
    public DialogueButton[] btn; 
}

[System.Serializable]
public class DialogueButton
{
    public string text;
    public int nextSeq;
}