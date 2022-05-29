using FCS_AlterraHub.Mono.Controllers;

namespace FCS_AlterraHub.Interfaces
{
    public interface IFCSDisplay
    {
        void GoToPage(int index);
        void GoToPage(int index,PaginatorController sender);
    }
}
