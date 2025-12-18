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
       <ul className={styles.links_list}>  
        <li>
          <NavLink to="/dashboard" className={({isActive}) => (isActive ? styles.active : '')}>
            Dashboard
          </NavLink>
        </li>
        <li>
          <NavLink to="/login"  className={({isActive}) => (isActive ? styles.active : '')}>
            Login
          </NavLink>
        </li>
        <li>
          <NavLink to="/register"  className={({isActive}) => (isActive ? styles.active : '')}>
            Register
          </NavLink>
        </li>
        <li>
          <NavLink to="/" className={styles.logout}>
            Logout
          </NavLink>
        </li>
       </ul>
    </nav>
  )
}

export default NavBar