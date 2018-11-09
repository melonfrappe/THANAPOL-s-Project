using System.Linq;

namespace UnityEngine.UI.Extensions.Examples
{
    public class Example01Scene : MonoBehaviour
    {
        [SerializeField] Example01ScrollView scrollView;
		[SerializeField] BookController bookController;

		void Update(){
			var cellDataIndex = Enumerable.Range(0, bookController.BookDataLength).Select(i => new Example01CellDto { Index = i }).ToList();
			scrollView.UpdateData(cellDataIndex);

		}
    }
}
