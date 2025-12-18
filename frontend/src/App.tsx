import { useState } from 'react'
import './App.css'

//components
import NavBar from './components/NavBar';
import Footer from './components/Footer';
import { Outlet } from 'react-router-dom';

//Works as a layout for the pages
//The pages routing is done at router.tsx
function App() {
  const [count, setCount] = useState(0)

  return (
    <div>
        <NavBar/>
        <Outlet />
         <button onClick={() => setCount((count) => count + 1)}>
          count is {count}
        </button>
        
        <Footer />
    </div>
  )
}

export default App
