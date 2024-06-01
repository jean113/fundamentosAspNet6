using System.ComponentModel.DataAnnotations;

namespace Blog.ViewModels.Accounts
{
    public class LoginViewModel
    {

        [Required(ErrorMessage ="O nome é obrigatório")]
        public string Name {get; set;}    

        [Required(ErrorMessage ="O email é obrigatório")]
        [EmailAddress(ErrorMessage ="O email é inválido")]
        public string Email {get; set;}   

        [Required(ErrorMessage ="Informe a senha")]
        public string Password {get; set;}   
    }

}