let rowCheckboxes = Array.from(document.getElementsByClassName('checkbox') );
let precisionCheckboxes = Array.from(document.getElementsByClassName('checkbox-precise') );
let searchBtn = document.getElementById('btn-search');

rowCheckboxes.forEach(checkbox => {
    checkbox.checked = true;
    checkbox.addEventListener('change', () => {
        onCheckBoxChange(checkbox);
    } );
} );

document.getElementById('btn-reset').addEventListener('click', () => {
    let inputs = Array.from(document.getElementsByTagName('input') );

    for(let i = 0; i < inputs.length; i++){
        if(rowCheckboxes.includes(inputs[i] ) ){
            inputs[i].checked = true;
            onCheckBoxChange(inputs[i] );
            continue;
        }
            
        inputs[i].value = '';
    }
} );

document.getElementById('btn-advanced').addEventListener('click', () => {
    let advancedPanel = document.querySelector('.advanced-control-row');
    let advancedPanelState = !advancedPanel.classList.contains('hidden');

    if(advancedPanelState){
        advancedPanel.classList.add('hidden');
        advancedPanel.classList.remove('anim');
        return;
    }
    
    advancedPanel.classList.remove('hidden');
    advancedPanel.classList.add('anim');
} );

searchBtn.addEventListener('click', () => {
    searchBtn.disabled = true;

    let checkboxesValues = {
        weight: rowCheckboxes.find(checkbox => checkbox.id.includes('weight')).checked,
        value: rowCheckboxes.find(checkbox => checkbox.id.includes('value')).checked,
        client: rowCheckboxes.find(checkbox => checkbox.id.includes('client')).checked,
        city: rowCheckboxes.find(checkbox => checkbox.id.includes('city')).checked,
        volumes: rowCheckboxes.find(checkbox => checkbox.id.includes('volumes')).checked,
        nfeNumber: rowCheckboxes.find(checkbox => checkbox.id.includes('nfe-number')).checked,
        shippingCompany: rowCheckboxes.find(checkbox => checkbox.id.includes('shipping-company')).checked,
        date: rowCheckboxes.find(checkbox => checkbox.id.includes('date')).checked
    }
    let preciseCheckboxes = {
        weight: precisionCheckboxes.find(checkbox => checkbox.id.includes('weight')).checked,
        value: precisionCheckboxes.find(checkbox => checkbox.id.includes('value')).checked,
        client: precisionCheckboxes.find(checkbox => checkbox.id.includes('client')).checked,
        city: precisionCheckboxes.find(checkbox => checkbox.id.includes('city')).checked,
        volumes: precisionCheckboxes.find(checkbox => checkbox.id.includes('volumes')).checked,
        nfeNumber: precisionCheckboxes.find(checkbox => checkbox.id.includes('nfe-number')).checked,
        shippingCompany: precisionCheckboxes.find(checkbox => checkbox.id.includes('shipping-company')).checked
    }
    let values = {
        toSearch: {
            weight: {
                value: checkboxesValues.weight === true ? document.getElementById('input-nfe-weight').value : '',
                precise: preciseCheckboxes.weight
            },
            value: {
                value: checkboxesValues.value === true ? document.getElementById('input-nfe-value').value : '',
                precise: preciseCheckboxes.value
            },
            client: {
                value: checkboxesValues.client === true ? document.getElementById('input-nfe-client').value : '',
                precise: preciseCheckboxes.client
            },
            city: {
                value: checkboxesValues.city === true ? document.getElementById('input-nfe-city').value : '',
                precise: preciseCheckboxes.city
            },
            volumes: {
                value: checkboxesValues.volumes === true ? document.getElementById('input-nfe-volumes').value : '',
                precise: preciseCheckboxes.volumes
            },
            nfeNumber: {
                value: checkboxesValues.nfeNumber === true ? document.getElementById('input-nfe-number').value : '',
                precise: preciseCheckboxes.nfeNumber
            },
            shippingCompany: {
                value: checkboxesValues.shippingCompany === true ? document.getElementById('input-nfe-shipping-company').value : '',
                precise: preciseCheckboxes.shippingCompany
            },
            date: {
                value: checkboxesValues.date === true ? document.getElementById('input-nfe-date').value : '',
                precise: false
            },
            summaryDateFilter: {
                from: document.querySelector('#limit-from-date').value,
                to: document.querySelector('#limit-to-date').value
            }
        }
    }
    
    if(values.toSearch.date.value !== '')
        values.toSearch.date.value = `${values.toSearch.date.value.substring(8, 10)}/${values.toSearch.date.value.substring(5, 7)}/${values.toSearch.date.value.substring(0, 4)}`;
    if(values.toSearch.summaryDateFilter.from !== '')
        values.toSearch.summaryDateFilter.from = `${values.toSearch.summaryDateFilter.from.substring(8, 10)}/${values.toSearch.summaryDateFilter.from.substring(5, 7)}/${values.toSearch.summaryDateFilter.from.substring(0, 4)}`;
    if(values.toSearch.summaryDateFilter.to !== '')
        values.toSearch.summaryDateFilter.to = `${values.toSearch.summaryDateFilter.to.substring(8, 10)}/${values.toSearch.summaryDateFilter.to.substring(5, 7)}/${values.toSearch.summaryDateFilter.to.substring(0, 4)}`;
    
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