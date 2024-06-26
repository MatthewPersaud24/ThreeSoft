namespace ThreeSoft.Models
{
    public class NoteModel
    {
        public int Id { get; set; }
        public string Content { get; set; }
        public string Sender { get; set; } // Teacher or Student
    }
}