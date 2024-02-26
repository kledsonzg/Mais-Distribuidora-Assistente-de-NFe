let checkboxes = Array.from(document.getElementsByClassName('checkbox') );
let searchBtn = document.getElementById('btn-search');

checkboxes.forEach(checkbox => {
    checkbox.checked = true;
    checkbox.addEventListener('change', () => {
        onCheckBoxChange(checkbox);
    } );
} );

document.getElementById('btn-reset').addEventListener('click', () => {
    let inputs = Array.from(document.getElementsByTagName('input') );

    for(let i = 0; i < inputs.length; i++){
        if(checkboxes.includes(inputs[i] ) ){
            inputs[i].checked = true;
            onCheckBoxChange(inputs[i] );
            continue;
        }
            
        inputs[i].value = '';
    }
} );

searchBtn.addEventListener('click', () => {
    searchBtn.disabled = true;

    let checkboxesValues = {
        weight: checkboxes.find(checkbox => checkbox.id.includes('weight')).checked,
        value: checkboxes.find(checkbox => checkbox.id.includes('value')).checked,
        client: checkboxes.find(checkbox => checkbox.id.includes('client')).checked,
        city: checkboxes.find(checkbox => checkbox.id.includes('city')).checked,
        volumes: checkboxes.find(checkbox => checkbox.id.includes('volumes')).checked,
        nfeNumber: checkboxes.find(checkbox => checkbox.id.includes('nfe-number')).checked,
        shippingCompany: checkboxes.find(checkbox => checkbox.id.includes('shipping-company')).checked,
        date: checkboxes.find(checkbox => checkbox.id.includes('date')).checked
    }
    let values = {
        toSearch: {
            weight: checkboxesValues.weight === true ? document.getElementById('input-nfe-weight').value : '',
            value: checkboxesValues.value === true ? document.getElementById('input-nfe-value').value : '',
            client: checkboxesValues.client === true ? document.getElementById('input-nfe-client').value : '',
            city: checkboxesValues.city === true ? document.getElementById('input-nfe-city').value : '',
            volumes: checkboxesValues.volumes === true ? document.getElementById('input-nfe-volumes').value : '',
            nfeNumber: checkboxesValues.nfeNumber === true ? document.getElementById('input-nfe-number').value : '',
            shippingCompany: checkboxesValues.shippingCompany === true ? document.getElementById('input-nfe-shipping-company').value : '',
            date: checkboxesValues.date === true ? document.getElementById('input-nfe-date').value : ''
        }
    }
    if(values.toSearch.date !== '')
        values.toSearch.date = `${values.toSearch.date.substring(8, 10)}/${values.toSearch.date.substring(5, 7)}/${values.toSearch.date.substring(0, 4)}`;
    console.log(values);
    console.log(values.toSearch.date);
    let req = new XMLHttpRequest();
    req.open('POST', `${window.location.origin}/search`);
    req.addEventListener('load', () => {
        console.log('uai!');
        searchBtn.disabled = false;
        if(req.status === 404){
            alert('NFe nao encontrada.');
            return;
        }
        if(req.status !== 200)
            return;

        console.log('ok!');
    } );
    req.send(JSON.stringify(values) );
} );

function onCheckBoxChange(checkbox){
    let row = checkbox.parentElement.parentElement;
    let children = Array.from(row.getElementsByTagName('input') );

    if(checkbox.checked === false)
        row.style.backgroundColor = '#434242';
    else row.removeAttribute('style');

    for(let i = 0; i < children.length; i++){
        let child = children[i];     
        if(child === checkbox)
            continue;

        if(checkbox.checked === false){
            child.style.backgroundColor = '#434242';
            child.disabled = true;
            continue
        }
            
        child.removeAttribute('style');
        child.disabled = false;
    }
}