import {createBrowserRouter, Navigate, type RouteObject, RouterProvider} from "react-router-dom";
import Login from "./pages/Login.tsx";
import SignUp from "./pages/SignUp.tsx";
import Home from "./pages/Home.tsx";
import UploadSong from "./pages/UploadSong.tsx";
import MyMusic from "./pages/MyMusic";
import Sidebar from "./pages/Sidebar.tsx";
import EditSong from "./pages/EditSong.tsx";
import CreatePlaylist from "./pages/CreatePlaylist.tsx";
import EditPlaylist from "./pages/EditPlaylist.tsx";

const routes: RouteObject[] = [
    {
        path: "/",
        element: <Sidebar/>,
        children: [
            {
                index: true,
                element: <Navigate to="/home" replace />
            },
            {
                path: "login",
                element: <Login/>
            },
            {
                path: "signup",
                element: <SignUp/>
            },
            {
                path: "home",
                element: <Home/>
            },
            {
                path: "uploadSong",
                element: <UploadSong/>
            },
            {
                path: "myMusic",
                element: <MyMusic/>
            },
            {
                path: "editSong/:id",
                element: <EditSong/>
            },
            {
                path: "createPlaylist",
                element: <CreatePlaylist/>
            },
            {
                path: "editPlaylist/:id",
                element: <EditPlaylist/>
            }
        ]
    },
];

function App() {
    return <RouterProvider router={createBrowserRouter(routes)} />;
}

export default App;
