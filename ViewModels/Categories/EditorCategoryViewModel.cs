using System.ComponentModel.DataAnnotations;

namespace Blog.ViewModels.Categories
{
    //è chamado de Editor porque esta classe serve tanto para o create quanto o update
    public class EditorCategoryViewModel
    {
      [Required(ErrorMessage = "O nome é obrigatório")]
      [StringLength(40, MinimumLength = 3, ErrorMessage = "Este campo de conter entre 3 a 40 caracteres")]
      public string Name { get; set; }  

      [Required(ErrorMessage = "O slug é obrigatório")]
      public string Slug { get; set; }
    }
}