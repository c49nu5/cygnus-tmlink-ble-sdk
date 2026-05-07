import './style.css'
import { CygnusBLE } from './cygnus-1ex-ble.ts'

document.querySelector<HTMLDivElement>('#app')!.innerHTML = `
    <button id="connect">Connect with gauge</button>
    <h2>Services</h2>
    <select id="serviceSelect"></select>
    <div id="characteristics"></div>
    </div>
    <textarea id="log"></textarea>
`

const cygnusBLE = new CygnusBLE();

document.getElementById("connect")!.addEventListener('click', function() {
    cygnusBLE.connect()
})

document.getElementById("serviceSelect")!.addEventListener('change', function() {    
    cygnusBLE.getCharacteristics()
})
