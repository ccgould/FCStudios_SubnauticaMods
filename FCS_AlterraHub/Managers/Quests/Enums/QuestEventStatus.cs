namespace FCS_AlterraHub.Managers.Quests.Enums
{
    /*
     * WAITING - not yet completed but can't be worked on cause there's a prerequisite event
     * CURRENT - the one the player should be trying to achieve
     * DONE - has been achieved
     */
    public enum QuestEventStatus
    {
        WAITING, 
        CURRENT, 
        DONE
    };
}
