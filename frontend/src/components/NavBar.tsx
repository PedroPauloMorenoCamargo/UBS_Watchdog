import { NavLink } from "react-router-dom"

// import { useAuthentication } from "../hooks/useAuthentication"
// import { useAuthValue } from "../context/AuthContext"


import styles from "./NavBar.module.css"

const NavBar = () => {

//   const { user } = useAuthValue();
//   const { logout } = useAuthentication()


  return (

    <nav className={styles.navbar}>
      <NavLink to="/" className={styles.brand}>
         UBS <span>Watchdog</span>
       </NavLink>
       <NavLink to="/dashboard">
        Dashboard
       </NavLink>
       <NavLink to="/login">
        Login
       </NavLink>
      <h2>TODO: Implementar Navbar</h2>
    </nav>
  )
}

export default NavBar