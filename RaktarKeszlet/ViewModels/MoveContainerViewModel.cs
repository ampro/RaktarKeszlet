using System.ComponentModel.DataAnnotations;

namespace RaktarKeszlet.ViewModels
{
    public class MoveContainerViewModel
    {
        [Required(ErrorMessage = "Kérlek, válaszd ki a mozgatni kívánt dobozt vagy raklapot!")]
        public int SelectedContainerId { get; set; }

        [Required(ErrorMessage = "A cél cég/tulajdonos megadása kötelező!")]
        public int TargetCompanyId { get; set; }

        public int? TargetBuildingId { get; set; }
        public int? TargetRoomId { get; set; }
        public int? TargetShelfId { get; set; }
    }
}