using System.Linq;

namespace UnityEngine.UI.Extensions.Examples
{
    public class Example01Scene : MonoBehaviour
    {
        [SerializeField] Example01ScrollView scrollView;
		[SerializeField] BookController bookController;

		bool isDone = false;

        void Start()
        {
            
        }

		void Update(){
			if(bookController.finishToRead && !isDone){
				var cellDataIndex = Enumerable.Range(0, bookController.BookDataLength).Select(i => new Example01CellDto { Index = i }).ToList();
				scrollView.UpdateData(cellDataIndex);
				isDone = true;
			}

		}
    }
}
