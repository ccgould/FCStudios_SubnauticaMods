namespace FCS_AlterraHub.Managers.Quests
{
    public class QuestPath
    {
        public QuestEvent StartEvent { get; set; }
        public QuestEvent EndEvent { get; set; }

        public QuestPath(QuestEvent from, QuestEvent to)
        {
            StartEvent = from;
            EndEvent = to;
        }
    }
}
