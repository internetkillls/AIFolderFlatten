// Static end-to-end test of the bundled browser script without a network or DOM package.
// Run: node scripts/smoke-cockpit-ui.js
'use strict';
const fs=require('fs'),vm=require('vm'),path=require('path'),assert=require('assert');
const source=fs.readFileSync(path.join(__dirname,'../src/AIFolderFlatten/TaskContext/CockpitRenderer.cs'),'utf8');
const match=source.match(/const string html = """\r?\n([\s\S]*?)\r?\n""";/);
assert(match,'Cockpit HTML template must be present');
const script=match[1].match(/<script>\r?\n([\s\S]*?)\r?\n<\/script>/);
assert(script,'Cockpit app script missing');
new vm.Script(script[1]);
const bundle={Kind:'aifolderflatten-task-context',Task:'Fix SettingsTab controls',EligibleCandidates:3,PolicyExclusions:1,SelectedCount:2,RedactionEnabled:true,BudgetChars:10000,
Files:[{Path:'SettingsTab.cs',State:'SELECTED',Score:80,Reasons:['Relevant settings source'],EvidenceLines:[2],OriginalSha256:'a'.repeat(64),ExportSha256:'b'.repeat(64),Redacted:false,Content:'class SettingsTab {}'},
{Path:'SettingsTabTests.cs',State:'SELECTED',Score:50,Reasons:['Related test'],EvidenceLines:[4],OriginalSha256:'c'.repeat(64),ExportSha256:'d'.repeat(64),Redacted:false,Content:'class SettingsTabTests {}'},
{Path:'Other.txt',State:'NOT_RELEVANT',Score:0,Reasons:[],EvidenceLines:[],OriginalSha256:null,ExportSha256:null,Redacted:false,Content:null}]};
class Node {
 constructor(id=''){this.id=id;this.children=[];this.style={};this.value='';this.textContent='';this.events={};this.checked=false;}
 append(...nodes){this.children.push(...nodes);}
 appendChild(x){this.append(x);return x;}
 replaceChildren(...nodes){this.children=[...nodes];}
 addEventListener(t,fn){this.events[t]=fn;}
 click(){if(this.events.click)this.events.click();}
 remove(){}
}
const elements={};const get=id=>elements[id]??(elements[id]=new Node(id));
get('context-data').textContent=JSON.stringify(bundle);
const document={getElementById:get,createElement:()=>new Node(),body:new Node()};
const ctx={document,navigator:{clipboard:{writeText:async()=>{}}},Intl,console,Blob,URL:{createObjectURL:()=> 'blob:example',revokeObjectURL:()=>{}},setTimeout:()=>{}};
vm.runInNewContext(script[1],ctx,{timeout:2000});
assert.strictEqual(get('filesCount').textContent,'2');
assert.strictEqual(get('omittedCount').textContent,'2');
assert.strictEqual(get('catalog').children.length,3);
const first=get('catalog').children[0];first.events.click();
assert(get('preview').textContent.includes('SettingsTab'));
first.children[0].checked=false;first.children[0].events.change();
assert.strictEqual(get('filesCount').textContent,'1');
assert.strictEqual(get('charsCount').textContent,String(bundle.Files[1].Content.length));
get('download').events.click();
console.log('PASS: cockpit JavaScript parses, renders candidates, previews evidence, toggles selection, updates budget stats, and prepares download');
