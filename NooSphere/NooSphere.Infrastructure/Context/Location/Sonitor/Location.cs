namespace ABC.Infrastructure.Context.Location.Sonitor
{
    public class GenericLocation<T>
    {
        public T X { get; set; }
        public T Y { get; set; }

        public GenericLocation( T x, T y )
        {
            X = x;
            Y = y;
        }
    }
}